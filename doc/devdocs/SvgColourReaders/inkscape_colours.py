# What Inkscape 1.2 or 1.4 shows for the colours of an SVG file's elements, on the CMS page of its Fill and Stroke
# dialog: the profile, and the channels with their values. Run it in an X server and a D-Bus session of its own, as
# run-all.sh does:
#   dbus-run-session -- xvfb-run -a -s "-screen 0 1400x900x24" python3 -I inkscape_colours.py <out folder> <file.svg> <probe>...
# A probe is an element's id, or o<n> for the first element whose data-o is n, or o<n>^ for that element's group,
# o<n>^^ for the group around that, and so on,
# then ":fill" or ":stroke". Inkscape is
# started with a preferences file of its own that opens colours on the CMS page, the dialog is opened and each element
# selected through Inkscape's actions on D-Bus, and the dialog is captured with ImageMagick's import and read with
# Tesseract. <out folder>/<name>.json holds the readings, and <name>-<probe>.png each captured dialog, to check the
# reading by eye. Needs inkscape, xdotool, gdbus, ImageMagick and tesseract. INKSCAPE names another Inkscape than the
# one on the path, such as a wrapper that runs Inkscape 1.4 in a chroot; its version picks the dialog's layout.
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
# Inkscape 1.4 shows the profile's name further left on the CMS page, its tabs closer together, and the channels'
# sliders so close to their labels that a wider crop takes in the slider of a dark colour.
PROFILE_1_4 = (230, 28, 115, 128)
LABELS_1_4 = (35, 200, 75, 150)
TABS_1_4 = {"fill": (1030, 170), "stroke": (1130, 170)}
INKSCAPE = os.environ.get("INKSCAPE", "inkscape")


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


# A copy of the file beside it in which each o<n> probe's element carries the id probe-o<n>, and each o<n>^ probe's
# group the id probe-o<n>-group, o<n>^^'s probe-o<n>-group-group and so on. The export writes an element or a group's
# start tag a line.
def probe_copy(svgPath, probes):
    lines = open(svgPath, encoding="utf-8").read().split("\n")
    for probe in probes:
        element = probe.split(":")[0]
        match = re.fullmatch(r"o(\d+)(\^*)", element)
        if not match:
            continue
        groups = []
        for index, line in enumerate(lines):
            if re.match(r"<g[ >]", line) and not line.endswith("/>"):
                groups.append(index)
            elif line == "</g>" and groups:
                groups.pop()
            elif ' data-o="%s"' % match.group(1) in line:
                target = groups[-len(match.group(2))] if match.group(2) else index
                lines[target] = re.sub(r"^<(\w+)", '<\\1 id="probe-%s"' % element.replace("^", "-group"), lines[target],
                                      count=1)
                break
    copyPath = svgPath[:-len(".svg")] + ".probe.svg"
    open(copyPath, "w", encoding="utf-8").write("\n".join(lines))
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
    version = re.search(r"Inkscape (\d+)\.(\d+)", run(INKSCAPE, "--version"))
    newer = version is not None and (int(version.group(1)), int(version.group(2))) >= (1, 4)
    profileBox, labelBox, tabs = (PROFILE_1_4, LABELS_1_4, TABS_1_4) if newer else (PROFILE, LABELS, TABS)
    inkscape = subprocess.Popen([INKSCAPE, "--with-gui", copyPath], env=environment,
                                stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
    readings = {"file": os.path.basename(svgPath), "inkscape": version.group(0) if version else "?", "probes": {}}
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
            elementId = "probe-" + element.replace("^", "-group") if re.fullmatch(r"o\d+\^*", element) else element
            activate("select-clear")
            activate("select-by-id", elementId)
            time.sleep(2)
            run("xdotool", "mousemove", str(tabs[tab][0]), str(tabs[tab][1]), "click", "1")
            time.sleep(2)
            imagePath = os.path.join(outFolder, "%s-%s-%s.png" % (name, element.replace("^", "-group"), tab))
            subprocess.run(["import", "-window", "root", "-crop", "%dx%d+%d+%d" % DIALOG, "+repage", imagePath],
                           check=True)
            labels = read_text(imagePath, labelBox, "CMYKRGBA:")
            values = read_text(imagePath, VALUES, "0123456789.")
            channels = [[label.rstrip(":"), value] for label, value in zip(labels, values)
                        if label.rstrip(":") in ("C", "M", "Y", "K", "R", "G", "B")]
            profile = read_text(imagePath, profileBox, None)
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
