// CourseAppearanceDialogViewModelTests.cs
//
// Tests for the blend mode in CourseAppearanceDialogViewModel: how the Settings
// property assembles and decomposes it, when the dialog lets it be chosen, and
// the blending that the preview sample is drawn with.

using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using PurplePen;
using PurplePen.Graphics2D;
using PurplePen.ViewModels;

namespace PurplePenViewModels.Tests
{
    /// <summary>
    /// Tests for the blend mode of the Customize Appearance dialog's ViewModel.
    /// </summary>
    [TestFixture]
    public class CourseAppearanceDialogViewModelTests
    {
        /// <summary>A new appearance blends by darken, and the dialog shows darken.</summary>
        [Test]
        public void NewAppearanceBlendsByDarken()
        {
            CourseAppearanceDialogViewModel vm = new CourseAppearanceDialogViewModel();

            Assert.That(vm.BlendModeIndex, Is.EqualTo(0));
            Assert.That(vm.Settings.overprintBlendMode, Is.EqualTo(BlendMode.Darken));
        }

        /// <summary>The Settings property decomposes the blend mode into the combo box's index, and assembles it again.</summary>
        /// <param name="blendMode">The blend mode of the appearance.</param>
        /// <param name="blendModeIndex">The index of the combo box item that shows it.</param>
        [TestCase(BlendMode.Darken, 0)]
        [TestCase(BlendMode.Multiply, 1)]
        public void SettingsCarryTheBlendMode(BlendMode blendMode, int blendModeIndex)
        {
            CourseAppearance appearance = new CourseAppearance();
            appearance.overprintBlendMode = blendMode;

            CourseAppearanceDialogViewModel vm = new CourseAppearanceDialogViewModel { Settings = appearance };

            Assert.That(vm.BlendModeIndex, Is.EqualTo(blendModeIndex));
            Assert.That(vm.Settings.overprintBlendMode, Is.EqualTo(blendMode));
        }

        /// <summary>
        /// The blend mode can be chosen where colors blend: where the purple blends, over any map, and where the colors
        /// of an OCAD map marked overprint use the overprint effect.
        /// </summary>
        /// <param name="usesOcadMap">Whether the map is an OCAD map.</param>
        /// <param name="blendPurpleIndex">How the purple blends: 0 not at all, 1 blended, 2 as a layer.</param>
        /// <param name="useOverprint">Whether the overprint effect is on.</param>
        /// <param name="enabled">Whether the blend mode can be chosen.</param>
        [TestCase(false, 0, false, false)]
        [TestCase(false, 1, false, true)]
        [TestCase(false, 0, true, false)]
        [TestCase(true, 0, false, false)]
        [TestCase(true, 2, false, false)]
        [TestCase(true, 2, true, true)]
        [TestCase(true, 1, true, true)]
        public void BlendModeIsEnabledWhereColorsBlend(bool usesOcadMap, int blendPurpleIndex, bool useOverprint, bool enabled)
        {
            CourseAppearanceDialogViewModel vm = new CourseAppearanceDialogViewModel {
                UsesOcadMap = usesOcadMap,
                BlendPurpleIndex = blendPurpleIndex,
                UseOverprint = useOverprint,
            };

            Assert.That(vm.BlendModeEnabled, Is.EqualTo(enabled));
        }

        /// <summary>Turning the overprint effect on enables the blend mode, and tells the view so.</summary>
        [Test]
        public void OverprintEffectEnablesTheBlendMode()
        {
            CourseAppearanceDialogViewModel vm = new CourseAppearanceDialogViewModel { UsesOcadMap = true, BlendPurpleIndex = 0 };
            List<string?> changedProperties = new List<string?>();
            vm.PropertyChanged += (sender, e) => changedProperties.Add(e.PropertyName);

            vm.UseOverprint = true;

            Assert.That(vm.BlendModeEnabled, Is.True);
            Assert.That(changedProperties, Does.Contain(nameof(CourseAppearanceDialogViewModel.BlendModeEnabled)));
        }

        /// <summary>The preview blends the purple by the chosen blend mode where the purple blends, and not otherwise.</summary>
        /// <param name="blendPurpleIndex">How the purple blends: 0 not at all, 1 blended, 2 as a layer.</param>
        /// <param name="blendModeIndex">The index of the chosen blend mode: 0 darken, 1 multiply.</param>
        /// <param name="pushedBlendModes">The blend modes the preview pushes.</param>
        [TestCase(1, 0, new[] { BlendMode.Darken })]
        [TestCase(1, 1, new[] { BlendMode.Multiply })]
        [TestCase(0, 1, new BlendMode[0])]
        [TestCase(2, 1, new BlendMode[0])]
        public void PreviewBlendsByTheChosenBlendMode(int blendPurpleIndex, int blendModeIndex, BlendMode[] pushedBlendModes)
        {
            CourseAppearanceDialogViewModel vm = new CourseAppearanceDialogViewModel {
                UsesOcadMap = true,
                BlendPurpleIndex = blendPurpleIndex,
                BlendModeIndex = blendModeIndex,
            };
            IGraphicsTarget target = DispatchProxy.Create<IGraphicsTarget, BlendingRecorder>();

            vm.DrawSample(target);

            Assert.That(((BlendingRecorder)(object)target).PushedBlendModes, Is.EqualTo(pushedBlendModes));
        }

        /// <summary>
        /// A graphics target that draws nothing and records the blend modes pushed onto it, each of which it supports.
        /// </summary>
        public class BlendingRecorder : DispatchProxy
        {
            /// <summary>The blend modes pushed, in order.</summary>
            public List<BlendMode> PushedBlendModes { get; } = new List<BlendMode>();

            /// <summary>
            /// Record a PushBlending and report the blend mode as supported; return the default value of any other call.
            /// </summary>
            /// <param name="targetMethod">The method of IGraphicsTarget that was called.</param>
            /// <param name="args">Its arguments.</param>
            protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
            {
                if (targetMethod!.Name == nameof(IGraphicsTarget.PushBlending)) {
                    PushedBlendModes.Add((BlendMode)args![0]!);
                    return true;
                }
                Type returnType = targetMethod.ReturnType;
                return (returnType.IsValueType && returnType != typeof(void)) ? Activator.CreateInstance(returnType) : null;
            }
        }
    }
}
