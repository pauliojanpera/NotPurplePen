using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using PurplePen.MapModel;

namespace PurplePen
{
    // The role of a place where a course formatter makes a course object: what the place makes of the data of EventDB.
    // Two places of the code that make the same thing have the same role (CourseObjKey).
    public enum CourseObjRole
    {
        Special,                // A special (CourseFormatter.CreateSpecial).
        Description,            // A description special (CourseFormatter.CreateDescriptionSpecial).
        Control,                // A control, start, finish, map exchange, crossing point or map issue point of a course view.
        ExtraCourseControl,     // The finish of a course that a part of the course shows (CourseView.ExtraCourseControls).
        Label,                  // The number or the code of a control, placed by the user or automatically.
        Leg,                    // A whole leg, flagged or not.
        LegBeginning,           // The beginning of a leg that is flagged in part.
        LegEnd,                 // The end of a leg that is flagged in part.
        LegMapIssuePoint,       // The map issue point along a leg (FlaggingKind.IssuePointMiddle).
        TopologyControl,        // A control of the topology view (TopologyFormatter.CreateControlNumber).
        TopologyLeg,            // A leg of the topology view (TopologyFormatter.CreateLegBetweenControls).
        DropTarget,             // A drop target along a leg of the topology view (TopologyDropTargetCourseObj).
        VariationCode,          // The code of a variation at the start of a fork (TopologyFormatter.CreateVariationCode).
    }

    // A course object key: the identity of a course object (CourseObj.Key). It has three parts: the role of the place of
    // a course formatter that made the course object, the identifiers of EventDB that the place uses, and the layer of
    // the course object. The class of the course object is not a part of it, and it is apart from the associations of
    // the course object that the selection uses (controlId, courseControlId, courseControlId2 and specialId). Its text
    // form is <role>.<identifier>….<layer>, with the numbers in base 62, for example leg.4.5.0, and two keys are the same
    // when their text forms are the same.
    public sealed class CourseObjKey : IEquatable<CourseObjKey>
    {
        private readonly CourseObjRole role;
        private readonly ReadOnlyCollection<int> identifiers;
        private readonly CourseLayer layer;
        private readonly string text;

        // Make a course object key.
        // role: the role of the place that made the course object.
        // identifiers: the identifiers of EventDB that the place uses, each 0 or more.
        // layer: the layer of the course object.
        public CourseObjKey(CourseObjRole role, IEnumerable<int> identifiers, CourseLayer layer)
        {
            if (identifiers == null)
                throw new ArgumentNullException(nameof(identifiers));

            this.role = role;
            this.identifiers = identifiers.ToList().AsReadOnly();
            this.layer = layer;

            string roleName = role.ToString();
            IEnumerable<string> parts = new string[] { char.ToLowerInvariant(roleName[0]) + roleName.Substring(1) }
                .Concat(this.identifiers.Select(identifier => SymbolKey.ToBase62(identifier)))
                .Concat(new string[] { SymbolKey.ToBase62((int)layer) });
            this.text = string.Join(".", parts);
        }

        // The role of the place that made the course object.
        public CourseObjRole Role
        {
            get { return role; }
        }

        // The identifiers of EventDB that the place uses.
        public IReadOnlyList<int> Identifiers
        {
            get { return identifiers; }
        }

        // The layer of the course object.
        public CourseLayer Layer
        {
            get { return layer; }
        }

        // The text form of the key.
        public string Text
        {
            get { return text; }
        }

        public bool Equals(CourseObjKey other)
        {
            return !ReferenceEquals(other, null) && string.Equals(text, other.text, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as CourseObjKey);
        }

        public override int GetHashCode()
        {
            return StringComparer.Ordinal.GetHashCode(text);
        }

        public static bool operator ==(CourseObjKey key1, CourseObjKey key2)
        {
            return ReferenceEquals(key1, null) ? ReferenceEquals(key2, null) : key1.Equals(key2);
        }

        public static bool operator !=(CourseObjKey key1, CourseObjKey key2)
        {
            return !(key1 == key2);
        }

        public override string ToString()
        {
            return text;
        }
    }
}
