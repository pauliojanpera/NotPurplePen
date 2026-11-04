#!/bin/bash
# inkscape-in-chroot.sh <arguments>: runs the Inkscape of a chroot in this X server and D-Bus session, for
# inkscape_colours.py to read another version than the system's (INKSCAPE=<this script>). The chroot, named by
# INKSCAPE_CHROOT, needs /tmp, /dev and /proc of the host bound in, so that it reaches the X server's socket, the files
# and the reader's preferences. Inkscape 1.4.2 came so from Ubuntu 25.10's archive, through debootstrap.
exec chroot "${INKSCAPE_CHROOT:?name the chroot}" /usr/bin/env -i PATH=/usr/bin:/bin DISPLAY="${DISPLAY:-}" \
    XAUTHORITY="${XAUTHORITY:-}" DBUS_SESSION_BUS_ADDRESS="${DBUS_SESSION_BUS_ADDRESS:-}" \
    XDG_CONFIG_HOME="${XDG_CONFIG_HOME:-}" HOME=/root LANG=C.UTF-8 inkscape "$@"
