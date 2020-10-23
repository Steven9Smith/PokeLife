using NUnit.Framework;
using System;
using Bee.Core;
using Unity.Build.Classic.Private;
using Unity.Build.RealPlatform;
using Unity.Build.RealPlatform.Classic;
using UnityEditor;


namespace Unity.Build.RealPlatform.Classic
{
    public class RealPlatform : Platform
    {
        public override bool HasPosix => false;

        public static RealPlatform Instance = new RealPlatform();
    }

    class RealPlatformClassicNonIncrementalPipeline : ClassicNonIncrementalPipelineBase
    {
        protected override RunResult OnRun(RunContext context)
        {
            throw new NotImplementedException();
        }

        protected override BuildTarget BuildTarget { get; } = BuildTarget.NoTarget;
        public override Platform Platform { get; } = RealPlatform.Instance;
    }
}

namespace Unity.Build.RealPlatform
{
    class RealPlatformClassicNonIncrementalPipelineWrongNamespace : ClassicNonIncrementalPipelineBase
    {
        protected override RunResult OnRun(RunContext context)
        {
            throw new NotImplementedException();
        }

        protected override BuildTarget BuildTarget { get; } = BuildTarget.NoTarget;
        public override Platform Platform { get; } = RealPlatform.Classic.RealPlatform.Instance;
    }
}

namespace Unity.Build.Classic.Tests
{
    class FakeClassicNonIncrementalPipeline : ClassicNonIncrementalPipelineBase
    {
        protected override RunResult OnRun(RunContext context)
        {
            throw new NotImplementedException();
        }

        protected override BuildTarget BuildTarget { get; } = BuildTarget.NoTarget;
        public override Platform Platform { get; } = RealPlatform.Classic.RealPlatform.Instance;
    }

    /// <summary>
    /// BuildPipelineSelector should only pick pipelines from namespace Unity.Build.*Platform*.Classic
    /// If pipeline class namespace contains "Test" word, ignore it
    /// </summary>
    [TestFixture]
    public class ClassicBuildPipelineTests
    {
        [Test]
        public void BuildPipelineSelectorTests()
        {
            Assert.IsTrue(BuildPipelineSelector.IsBuildPipelineValid(new RealPlatformClassicNonIncrementalPipeline(), RealPlatform.Classic.RealPlatform.Instance));
            Assert.IsFalse(BuildPipelineSelector.IsBuildPipelineValid(new FakeClassicNonIncrementalPipeline(), RealPlatform.Classic.RealPlatform.Instance));
            Assert.IsFalse(BuildPipelineSelector.IsBuildPipelineValid(new RealPlatformClassicNonIncrementalPipelineWrongNamespace(), RealPlatform.Classic.RealPlatform.Instance));

            var selector = new BuildPipelineSelector();
            Assert.AreEqual(selector.SelectFor(RealPlatform.Classic.RealPlatform.Instance, false).GetType(), typeof(RealPlatformClassicNonIncrementalPipeline));
        }
    }
}
