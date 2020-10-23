using NUnit.Framework;
using Unity.Collections;

namespace Unity.Entities.Tests
{
    class EntityManagerComponentGroupOperationsTests : ECSTestsFixture
    {
        [Test]
        public void AddRemoveChunkComponentWithGroupWorks()
        {
            var metaChunkGroup = m_Manager.CreateEntityQuery(typeof(ChunkHeader));

            var entity1 = m_Manager.CreateEntity(typeof(EcsTestData));
            var entity2 = m_Manager.CreateEntity(typeof(EcsTestData), typeof(EcsTestData2));
            var entity3 = m_Manager.CreateEntity(typeof(EcsTestData2));

            var group1 = m_Manager.CreateEntityQuery(ComponentType.ReadWrite<EcsTestData>());

            m_Manager.AddChunkComponentData(group1, new EcsTestData3(7));

            Assert.IsTrue(m_Manager.HasComponent(entity1, ComponentType.ChunkComponent<EcsTestData3>()));
            var val1 = m_Manager.GetChunkComponentData<EcsTestData3>(entity1).value0;
            Assert.AreEqual(7, val1);

            Assert.IsTrue(m_Manager.HasComponent(entity2, ComponentType.ChunkComponent<EcsTestData3>()));
            var val2 = m_Manager.GetChunkComponentData<EcsTestData3>(entity2).value0;
            Assert.AreEqual(7, val2);

            Assert.IsFalse(m_Manager.HasComponent(entity3, ComponentType.ChunkComponent<EcsTestData3>()));

            Assert.AreEqual(2, metaChunkGroup.CalculateEntityCount());

            m_ManagerDebug.CheckInternalConsistency();

            var group2 = m_Manager.CreateEntityQuery(ComponentType.ReadWrite<EcsTestData2>(), ComponentType.ChunkComponent<EcsTestData3>());

            m_Manager.RemoveChunkComponentData<EcsTestData3>(group2);

            Assert.IsFalse(m_Manager.HasComponent(entity2, ComponentType.ChunkComponent<EcsTestData3>()));

            Assert.AreEqual(1, metaChunkGroup.CalculateEntityCount());

            m_Manager.DestroyEntity(entity1);
            m_Manager.DestroyEntity(entity2);
            m_Manager.DestroyEntity(entity3);
            metaChunkGroup.Dispose();
            group1.Dispose();
            group2.Dispose();
        }

        [Test]
        public void AddRemoveSharedComponentWithGroupWorks()
        {
            var entity1 = m_Manager.CreateEntity(typeof(EcsTestData));
            var entity2 = m_Manager.CreateEntity(typeof(EcsTestData), typeof(EcsTestData2));
            var entity3 = m_Manager.CreateEntity(typeof(EcsTestData2));

            var group1 = m_Manager.CreateEntityQuery(ComponentType.ReadWrite<EcsTestData>());

            m_Manager.AddSharedComponentData(group1, new EcsTestSharedComp(7));

            Assert.IsTrue(m_Manager.HasComponent(entity1, ComponentType.ReadWrite<EcsTestSharedComp>()));
            var val1 = m_Manager.GetSharedComponentData<EcsTestSharedComp>(entity1).value;
            Assert.AreEqual(7, val1);

            Assert.IsTrue(m_Manager.HasComponent(entity2, ComponentType.ReadWrite<EcsTestSharedComp>()));
            var val2 = m_Manager.GetSharedComponentData<EcsTestSharedComp>(entity2).value;
            Assert.AreEqual(7, val2);

            Assert.IsFalse(m_Manager.HasComponent(entity3, ComponentType.ReadWrite<EcsTestSharedComp>()));

            m_ManagerDebug.CheckInternalConsistency();

            var group2 = m_Manager.CreateEntityQuery(ComponentType.ReadWrite<EcsTestData2>(), ComponentType.ReadWrite<EcsTestSharedComp>());

            m_Manager.RemoveComponent(group2, typeof(EcsTestSharedComp));

            Assert.IsFalse(m_Manager.HasComponent(entity2, typeof(EcsTestSharedComp)));
        }

        [Test]
        public void AddRemoveAnyComponentWithGroupWorksWithVariousTypes()
        {
            var componentTypes = new ComponentType[] { typeof(EcsTestTag), typeof(EcsTestData4), ComponentType.ChunkComponent<EcsTestData4>(), typeof(EcsTestSharedComp) };

            foreach (var type in componentTypes)
            {
                // We want a clean slate for the m_manager so teardown and setup before the test
                TearDown();
                Setup();

                var metaChunkGroup = m_Manager.CreateEntityQuery(typeof(ChunkHeader));

                var entity1 = m_Manager.CreateEntity(typeof(EcsTestData));
                var entity2 = m_Manager.CreateEntity(typeof(EcsTestData), typeof(EcsTestData2));
                var entity3 = m_Manager.CreateEntity(typeof(EcsTestData2));

                var group1 = m_Manager.CreateEntityQuery(ComponentType.ReadWrite<EcsTestData>());

                m_Manager.AddComponent(group1, type);


                Assert.IsTrue(m_Manager.HasComponent(entity1, type));
                Assert.IsTrue(m_Manager.HasComponent(entity2, type));
                Assert.IsFalse(m_Manager.HasComponent(entity3, type));

                if (type.IsChunkComponent)
                    Assert.AreEqual(2, metaChunkGroup.CalculateEntityCount());

                if (type == ComponentType.ReadWrite<EcsTestSharedComp>())
                {
                    m_Manager.SetSharedComponentData(entity1, new EcsTestSharedComp(1));
                    m_Manager.SetSharedComponentData(entity2, new EcsTestSharedComp(2));
                }

                m_ManagerDebug.CheckInternalConsistency();

                var group2 = m_Manager.CreateEntityQuery(ComponentType.ReadWrite<EcsTestData2>(), type);

                m_Manager.RemoveComponent(group2, type);

                Assert.IsFalse(m_Manager.HasComponent(entity2, ComponentType.ChunkComponent<EcsTestData3>()));

                if (type.IsChunkComponent)
                    Assert.AreEqual(1, metaChunkGroup.CalculateEntityCount());
            }
        }

        [Test]
        [IgnoreInPortableTests("intermittent crash (likely race condition)")]
        public void RemoveAnyComponentWithGroupIgnoresChunksThatDontHaveTheComponent()
        {
            var componentTypes = new ComponentType[]
            {
                typeof(EcsTestTag), typeof(EcsTestData4), ComponentType.ChunkComponent<EcsTestData4>(), typeof(EcsTestSharedComp)
            };

            foreach (var type in componentTypes)
            {
                // We want a clean slate for the m_manager so teardown and setup before the test
                TearDown();
                Setup();

                var metaChunkGroup = m_Manager.CreateEntityQuery(typeof(ChunkHeader));

                var entity1 = m_Manager.CreateEntity(typeof(EcsTestData));
                var entity2 = m_Manager.CreateEntity(typeof(EcsTestData), typeof(EcsTestData2));
                var entity3 = m_Manager.CreateEntity(typeof(EcsTestData2));

                var group1 = m_Manager.CreateEntityQuery(ComponentType.ReadWrite<EcsTestData>());

                m_Manager.AddComponent(group1, type);

                Assert.IsTrue(m_Manager.HasComponent(entity1, type));
                Assert.IsTrue(m_Manager.HasComponent(entity2, type));
                Assert.IsFalse(m_Manager.HasComponent(entity3, type));

                if (type.IsChunkComponent)
                    Assert.AreEqual(2, metaChunkGroup.CalculateEntityCount());

                if (type == ComponentType.ReadWrite<EcsTestSharedComp>())
                {
                    m_Manager.SetSharedComponentData(entity1, new EcsTestSharedComp(1));
                    m_Manager.SetSharedComponentData(entity2, new EcsTestSharedComp(2));
                }

                m_ManagerDebug.CheckInternalConsistency();

                m_Manager.RemoveComponent(m_Manager.UniversalQuery, type);

                Assert.AreEqual(0, m_Manager.CreateEntityQuery(type).CalculateEntityCount());
            }
        }

        uint GetComponentDataVersion<T>(Entity e) where T :
#if UNITY_DISABLE_MANAGED_COMPONENTS
        struct,
#endif
        IComponentData
        {
            return m_Manager.GetChunk(e).GetChangeVersion(m_Manager.GetComponentTypeHandle<T>(true));
        }

        uint GetSharedComponentDataVersion<T>(Entity e) where T : struct, ISharedComponentData
        {
            return m_Manager.GetChunk(e).GetChangeVersion(m_Manager.GetSharedComponentTypeHandle<T>());
        }

        [Test]
        public void AddRemoveComponentWithGroupPreservesChangeVersions()
        {
            m_ManagerDebug.SetGlobalSystemVersion(10);
            var entity1 = m_Manager.CreateEntity(typeof(EcsTestData));
            var entity2 = m_Manager.CreateEntity(typeof(EcsTestData), typeof(EcsTestData2));
            var entity3 = m_Manager.CreateEntity(typeof(EcsTestData2));

            m_ManagerDebug.SetGlobalSystemVersion(20);

            m_Manager.SetComponentData(entity2, new EcsTestData2(7));
            m_Manager.SetComponentData(entity3, new EcsTestData2(8));

            Assert.AreEqual(10, GetComponentDataVersion<EcsTestData>(entity1));
            Assert.AreEqual(10, GetComponentDataVersion<EcsTestData>(entity2));
            Assert.AreEqual(20, GetComponentDataVersion<EcsTestData2>(entity2));
            Assert.AreEqual(20, GetComponentDataVersion<EcsTestData2>(entity3));

            m_ManagerDebug.SetGlobalSystemVersion(30);

            m_Manager.AddSharedComponentData(m_Manager.UniversalQuery, new EcsTestSharedComp(1));
            m_ManagerDebug.SetGlobalSystemVersion(40);
            m_Manager.AddComponent(m_Manager.UniversalQuery, typeof(EcsTestTag));
            Assert.AreEqual(30, GetSharedComponentDataVersion<EcsTestSharedComp>(entity1));
            Assert.AreEqual(30, GetSharedComponentDataVersion<EcsTestSharedComp>(entity2));
            Assert.AreEqual(30, GetSharedComponentDataVersion<EcsTestSharedComp>(entity3));

            Assert.AreEqual(40, GetComponentDataVersion<EcsTestTag>(entity1));
            Assert.AreEqual(40, GetComponentDataVersion<EcsTestTag>(entity2));
            Assert.AreEqual(40, GetComponentDataVersion<EcsTestTag>(entity3));

            m_ManagerDebug.SetGlobalSystemVersion(50);

            m_Manager.RemoveComponent(m_Manager.UniversalQuery, typeof(EcsTestSharedComp2));
            Assert.AreEqual(30, GetSharedComponentDataVersion<EcsTestSharedComp>(entity1));
            Assert.AreEqual(30, GetSharedComponentDataVersion<EcsTestSharedComp>(entity2));
            Assert.AreEqual(30, GetSharedComponentDataVersion<EcsTestSharedComp>(entity3));

            m_ManagerDebug.SetGlobalSystemVersion(60);

            m_Manager.RemoveComponent(m_Manager.UniversalQuery, typeof(EcsTestSharedComp));
            Assert.AreEqual(10, GetComponentDataVersion<EcsTestData>(entity1));
            Assert.AreEqual(10, GetComponentDataVersion<EcsTestData>(entity2));
            Assert.AreEqual(20, GetComponentDataVersion<EcsTestData2>(entity2));
            Assert.AreEqual(20, GetComponentDataVersion<EcsTestData2>(entity3));
        }

#if !UNITY_DISABLE_MANAGED_COMPONENTS
        [Test]
        public void AddRemoveChunkComponentWithGroupWorks_ManagedComponents()
        {
            var metaChunkGroup = m_Manager.CreateEntityQuery(typeof(ChunkHeader));

            var entity1 = m_Manager.CreateEntity(typeof(EcsTestData));
            var entity2 = m_Manager.CreateEntity(typeof(EcsTestData), typeof(EcsTestData2));
            var entity3 = m_Manager.CreateEntity(typeof(EcsTestData2));

            var group1 = m_Manager.CreateEntityQuery(ComponentType.ReadWrite<EcsTestData>());

            m_ManagerDebug.CheckInternalConsistency();
            m_Manager.AddChunkComponentData(group1, new EcsTestManagedComponent() { value = "SomeString" });
            m_ManagerDebug.CheckInternalConsistency();

            Assert.IsTrue(m_Manager.HasComponent(entity1, ComponentType.ChunkComponent<EcsTestManagedComponent>()));
            var val1 = m_Manager.GetChunkComponentData<EcsTestManagedComponent>(entity1).value;
            Assert.AreEqual("SomeString", val1);

            Assert.IsTrue(m_Manager.HasComponent(entity2, ComponentType.ChunkComponent<EcsTestManagedComponent>()));
            var val2 = m_Manager.GetChunkComponentData<EcsTestManagedComponent>(entity2).value;
            Assert.AreEqual("SomeString", val2);

            Assert.IsFalse(m_Manager.HasComponent(entity3, ComponentType.ChunkComponent<EcsTestManagedComponent>()));

            Assert.AreEqual(2, metaChunkGroup.CalculateEntityCount());

            m_ManagerDebug.CheckInternalConsistency();

            var group2 = m_Manager.CreateEntityQuery(ComponentType.ReadWrite<EcsTestData2>(), ComponentType.ChunkComponent<EcsTestManagedComponent>());

            m_Manager.RemoveChunkComponentData<EcsTestManagedComponent>(group2);

            Assert.IsFalse(m_Manager.HasComponent(entity2, ComponentType.ChunkComponent<EcsTestManagedComponent>()));

            Assert.AreEqual(1, metaChunkGroup.CalculateEntityCount());

            m_Manager.DestroyEntity(entity1);
            m_Manager.DestroyEntity(entity2);
            m_Manager.DestroyEntity(entity3);
            metaChunkGroup.Dispose();
            group1.Dispose();
            group2.Dispose();
        }

        [Test]
        public void AddRemoveAnyComponentWithGroupWorksWithVariousTypes_ManagedComponents()
        {
            var componentTypes = new ComponentType[]
            {
                typeof(EcsTestTag), typeof(EcsTestData4), ComponentType.ChunkComponent<EcsTestData4>(), typeof(EcsTestSharedComp),
                typeof(EcsTestManagedComponent), ComponentType.ChunkComponent<EcsTestManagedComponent>()
            };

            foreach (var type in componentTypes)
            {
                // We want a clean slate for the m_manager so teardown and setup before the test
                TearDown();
                Setup();

                var metaChunkGroup = m_Manager.CreateEntityQuery(typeof(ChunkHeader));

                var entity1 = m_Manager.CreateEntity(typeof(EcsTestData));
                var entity2 = m_Manager.CreateEntity(typeof(EcsTestData), typeof(EcsTestData2));
                var entity3 = m_Manager.CreateEntity(typeof(EcsTestData2));

                var group1 = m_Manager.CreateEntityQuery(ComponentType.ReadWrite<EcsTestData>());

                m_Manager.AddComponent(group1, type);

                Assert.IsTrue(m_Manager.HasComponent(entity1, type));
                Assert.IsTrue(m_Manager.HasComponent(entity2, type));
                Assert.IsFalse(m_Manager.HasComponent(entity3, type));

                if (type.IsChunkComponent)
                    Assert.AreEqual(2, metaChunkGroup.CalculateEntityCount());

                if (type == ComponentType.ReadWrite<EcsTestSharedComp>())
                {
                    m_Manager.SetSharedComponentData(entity1, new EcsTestSharedComp(1));
                    m_Manager.SetSharedComponentData(entity2, new EcsTestSharedComp(2));
                }

                m_ManagerDebug.CheckInternalConsistency();

                var group2 = m_Manager.CreateEntityQuery(ComponentType.ReadWrite<EcsTestData2>(), type);

                m_Manager.RemoveComponent(group2, type);

                Assert.IsFalse(m_Manager.HasComponent(entity2, ComponentType.ChunkComponent<EcsTestData3>()));

                if (type.IsChunkComponent)
                    Assert.AreEqual(1, metaChunkGroup.CalculateEntityCount());
            }
        }

        [Test]
        [IgnoreInPortableTests("intermittent crash (likely race condition)")]
        public void RemoveAnyComponentWithGroupIgnoresChunksThatDontHaveTheComponent_ManagedComponents()
        {
            var componentTypes = new ComponentType[]
            {
                typeof(EcsTestTag), typeof(EcsTestData4), ComponentType.ChunkComponent<EcsTestData4>(), typeof(EcsTestSharedComp),
                typeof(EcsTestManagedComponent), ComponentType.ChunkComponent<EcsTestManagedComponent>()
            };

            foreach (var type in componentTypes)
            {
                // We want a clean slate for the m_manager so teardown and setup before the test
                TearDown();
                Setup();

                var metaChunkGroup = m_Manager.CreateEntityQuery(typeof(ChunkHeader));

                var entity1 = m_Manager.CreateEntity(typeof(EcsTestData));
                var entity2 = m_Manager.CreateEntity(typeof(EcsTestData), typeof(EcsTestData2));
                var entity3 = m_Manager.CreateEntity(typeof(EcsTestData2));

                var group1 = m_Manager.CreateEntityQuery(ComponentType.ReadWrite<EcsTestData>());

                m_Manager.AddComponent(group1, type);

                Assert.IsTrue(m_Manager.HasComponent(entity1, type));
                Assert.IsTrue(m_Manager.HasComponent(entity2, type));
                Assert.IsFalse(m_Manager.HasComponent(entity3, type));

                if (type.IsChunkComponent)
                    Assert.AreEqual(2, metaChunkGroup.CalculateEntityCount());

                if (type == ComponentType.ReadWrite<EcsTestSharedComp>())
                {
                    m_Manager.SetSharedComponentData(entity1, new EcsTestSharedComp(1));
                    m_Manager.SetSharedComponentData(entity2, new EcsTestSharedComp(2));
                }

                m_ManagerDebug.CheckInternalConsistency();

                m_Manager.RemoveComponent(m_Manager.UniversalQuery, type);

                Assert.AreEqual(0, m_Manager.CreateEntityQuery(type).CalculateEntityCount());
            }
        }

        [Test]
        public void AddRemoveComponentWithGroupPreservesChangeVersions_ManagedComponents()
        {
            m_ManagerDebug.SetGlobalSystemVersion(10);
            var entity1 = m_Manager.CreateEntity(typeof(EcsTestData));
            var entity2 = m_Manager.CreateEntity(typeof(EcsTestData), typeof(EcsTestData2));
            var entity3 = m_Manager.CreateEntity(typeof(EcsTestData2));
            var entity4 = m_Manager.CreateEntity(typeof(EcsTestData2), typeof(EcsTestManagedComponent));
            var entity5 = m_Manager.CreateEntity(typeof(EcsTestManagedComponent));

            m_ManagerDebug.SetGlobalSystemVersion(20);

            m_Manager.SetComponentData(entity2, new EcsTestData2(7));
            m_Manager.SetComponentData(entity3, new EcsTestData2(8));
            m_Manager.SetComponentData(entity4, new EcsTestData2(9));

            Assert.AreEqual(10, GetComponentDataVersion<EcsTestData>(entity1));
            Assert.AreEqual(10, GetComponentDataVersion<EcsTestData>(entity2));
            Assert.AreEqual(20, GetComponentDataVersion<EcsTestData2>(entity2));
            Assert.AreEqual(20, GetComponentDataVersion<EcsTestData2>(entity3));
            Assert.AreEqual(20, GetComponentDataVersion<EcsTestData2>(entity4));
            Assert.AreEqual(10, GetComponentDataVersion<EcsTestManagedComponent>(entity4));
            Assert.AreEqual(10, GetComponentDataVersion<EcsTestManagedComponent>(entity5));

            m_ManagerDebug.SetGlobalSystemVersion(30);

            m_Manager.AddSharedComponentData(m_Manager.UniversalQuery, new EcsTestSharedComp(1));

            m_ManagerDebug.SetGlobalSystemVersion(40);

            m_Manager.AddComponent(m_Manager.UniversalQuery, typeof(EcsTestTag));

            Assert.AreEqual(30, GetSharedComponentDataVersion<EcsTestSharedComp>(entity1));
            Assert.AreEqual(30, GetSharedComponentDataVersion<EcsTestSharedComp>(entity2));
            Assert.AreEqual(30, GetSharedComponentDataVersion<EcsTestSharedComp>(entity3));
            Assert.AreEqual(30, GetSharedComponentDataVersion<EcsTestSharedComp>(entity4));
            Assert.AreEqual(30, GetSharedComponentDataVersion<EcsTestSharedComp>(entity5));

            Assert.AreEqual(40, GetComponentDataVersion<EcsTestTag>(entity1));
            Assert.AreEqual(40, GetComponentDataVersion<EcsTestTag>(entity2));
            Assert.AreEqual(40, GetComponentDataVersion<EcsTestTag>(entity3));
            Assert.AreEqual(40, GetComponentDataVersion<EcsTestTag>(entity4));
            Assert.AreEqual(40, GetComponentDataVersion<EcsTestTag>(entity5));

            m_ManagerDebug.SetGlobalSystemVersion(50);

            m_Manager.RemoveComponent(m_Manager.UniversalQuery, typeof(EcsTestSharedComp2));

            Assert.AreEqual(30, GetSharedComponentDataVersion<EcsTestSharedComp>(entity1));
            Assert.AreEqual(30, GetSharedComponentDataVersion<EcsTestSharedComp>(entity2));
            Assert.AreEqual(30, GetSharedComponentDataVersion<EcsTestSharedComp>(entity3));
            Assert.AreEqual(30, GetSharedComponentDataVersion<EcsTestSharedComp>(entity4));
            Assert.AreEqual(30, GetSharedComponentDataVersion<EcsTestSharedComp>(entity5));

            m_ManagerDebug.SetGlobalSystemVersion(60);

            m_Manager.RemoveComponent(m_Manager.UniversalQuery, typeof(EcsTestSharedComp));

            Assert.AreEqual(10, GetComponentDataVersion<EcsTestData>(entity1));
            Assert.AreEqual(10, GetComponentDataVersion<EcsTestData>(entity2));
            Assert.AreEqual(20, GetComponentDataVersion<EcsTestData2>(entity2));
            Assert.AreEqual(20, GetComponentDataVersion<EcsTestData2>(entity3));
            Assert.AreEqual(20, GetComponentDataVersion<EcsTestData2>(entity4));
            Assert.AreEqual(10, GetComponentDataVersion<EcsTestManagedComponent>(entity4));
            Assert.AreEqual(10, GetComponentDataVersion<EcsTestManagedComponent>(entity5));
        }

#endif
    }
}