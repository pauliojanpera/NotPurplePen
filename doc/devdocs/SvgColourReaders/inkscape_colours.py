# What Inkscape 1.2 shows for the colours of an SVG file's elements, on the CMS page of its Fill and Stroke dialog: the
# profile, and the channels with their values. Run it in an X server and a D-Bus session of its own, as
# run-inkscape.sh does:
#   dbus-run-session -- xvfb-run -a -s "-screen 0 1400x900x24" python3 -I inkscape_colours.py <out folder> <file.svg> <probe>...
# A probe is an element's id, or o<n> for the first element whose data-o is n, then ":fill" or ":stroke". Inkscape is
# started with a preferences file of its own that opens colours on the CMS page, the dialog is opened and each element
# selected through Inkscape's actions on D-Bus, and the dialog is captured with ImageMagick's import and read with
# Tesseract. <out folder>/<name>.json holds the readings, and <name>-<probe>.png each captured dialog, to check the
# reading by eye. Needs inkscape, xdotool, gdbus, ImageMagick and tesseract.
import json
import os
import re
import subprocess
import sys
import tempfile
import time

PREFERENCES = """<?xml version="1.0" encoding="UTF-8" standalone="no"?>
<inkscape xmlns:inkscape="http://www.inkscape.org/namespaces/inkscape" version="1">
  <group id="colorselector" page="5" switcher="1" />
</inkscape>
"""

# Where the dialog lies on a 1400 by 900 screen, and its parts within it: (width, height, left, top).
DIALOG = (450, 560, 950, 150)
PROFILE = (210, 28, 135, 128)
LABELS = (50, 200, 75, 150)
VALUES = (42, 200, 357, 150)
TABS = {"fill": (1050, 170), "stroke": (1160, 170)}


def run(*command):
    return subprocess.run(command, capture_output=True, text=True).stdout


# Activate one of Inkscape's actions through D-Bus.
# path: "" for the application's actions, "/window/1" for the window's.
def activate(action, argument=None, path=""):
    parameter = "[<'%s'>]" % argument if argument is not None else "[]"
    run("gdbus", "call", "--session", "--dest", "org.inkscape.Inkscape", "--object-path", "/org/inkscape/Inkscape" + path,
        "--method", "org.gtk.Actions.Activate", action, parameter, "{}")


# The text Tesseract reads in a part of a captured dialog.
# whitelist: the characters it may read, or None for any.
def read_text(imagePath, box, whitelist):
    with tempfile.NamedTemporaryFile(suffix=".png") as part:
        subprocess.run(["convert", imagePath, "+repage", "-crop", "%dx%d+%d+%d" % box, "+repage", "-resize", "400%",
                        "-colorspace", "Gray", "-threshold", "55%", part.name], check=True)
        command = ["tesseract", part.name, "-", "--psm", "6"]
        if whitelist:
            command += ["-c", "tessedit_char_whitelist=" + whitelist]
        return run(*command).split()


# A copy of the file beside it in which each o<n> probe's element carries the id probe-o<n>.
def probe_copy(svgPath, probes):
    text = open(svgPath, encoding="utf-8").read()
    for probe in probes:
        element = probe.split(":")[0]
        if re.fullmatch(r"o\d+", element):
            text = re.sub(r'<(\w+)([^>]*?) data-o="%s"' % element[1:],
                          lambda match: '<%s id="probe-%s"%s data-o="%s"' % (match.group(1), element, match.group(2),
                                                                           element[1:]),
                          text, count=1)
    copyPath = svgPath[:-len(".svg")] + ".probe.svg"
    open(copyPath, "w", encoding="utf-8").write(text)
    return copyPath


def main():
    outFolder, svgPath, probes = sys.argv[1], os.path.abspath(sys.argv[2]), sys.argv[3:]
    os.makedirs(outFolder, exist_ok=True)
    name = os.path.splitext(os.path.basename(svgPath))[0]
    copyPath = probe_copy(svgPath, probes)
    configFolder = tempfile.mkdtemp()
    os.makedirs(os.path.join(configFolder, "inkscape"))
    open(os.path.join(configFolder, "inkscape", "preferences.xml"), "w").write(PREFERENCES)
    environment = dict(os.environ, XDG_CONFIG_HOME=configFolder)
    inkscape = subprocess.Popen(["inkscape", "--with-gui", copyPath], env=environment,
                                stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
    readings = {"file": os.path.basename(svgPath), "probes": {}}
    try:
        window = ""
        for _ in range(120):
            window = run("xdotool", "search", "--onlyvisible", "--class", "inkscape").split()
            if window:
                break
            time.sleep(1)
        time.sleep(8)
        run("xdotool", "windowsize", window[0], "1400", "900")
        time.sleep(2)
        activate("dialog-open", "FillStroke", "/window/1")
        time.sleep(4)
        for probe in probes:
            element, tab = probe.split(":")
            elementId = "probe-" + element if re.fullmatch(r"o\d+", element) else element
            activate("select-clear")
            activate("select-by-id", elementId)
            time.sleep(2)
            run("xdotool", "mousemove", str(TABS[tab][0]), str(TABS[tab][1]), "click", "1")
            time.sleep(2)
            imagePath = os.path.join(outFolder, "%s-%s-%s.png" % (name, element, tab))
            subprocess.run(["import", "-window", "root", "-crop", "%dx%d+%d+%d" % DIALOG, "+repage", imagePath],
                           check=True)
            labels = read_text(imagePath, LABELS, "CMYKRGBA:")
            values = read_text(imagePath, VALUES, "0123456789.")
            channels = [[label.rstrip(":"), value] for label, value in zip(labels, values) if label.rstrip(":") != "A"]
            profile = read_text(imagePath, PROFILE, None)
            while profile and re.fullmatch(r"[vV]+", profile[-1]):
                profile.pop()
            readings["probes"][probe] = {"profile": " ".join(profile), "channels": channels}
    finally:
        inkscape.kill()
        inkscape.wait()
        os.remove(copyPath)
    with open(os.path.join(outFolder, name + ".json"), "w") as out:
        json.dump(readings, out, indent=1)
    for probe, reading in readings["probes"].items():
        print("%s %s: profile %s, channels %s" % (name, probe, reading["profile"] or "-",
              " ".join("%s %s" % tuple(channel) for channel in reading["channels"]) or "none"))


main()
