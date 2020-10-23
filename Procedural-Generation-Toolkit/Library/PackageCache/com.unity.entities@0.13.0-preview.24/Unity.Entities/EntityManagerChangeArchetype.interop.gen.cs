
//------------------------------------------------------------------------------
// <auto-generated>
//     This file was automatically generated by Unity.Entities.Editor.BurstInteropCodeGenerator
//     Any changes you make here will be overwritten
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
//
//     To update this file, use the "DOTS -> Regenerate Burst Interop" menu option.
//
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using Unity.Burst;
using System.Runtime.InteropServices;

namespace Unity.Entities
{
     unsafe partial struct StructuralChange
    {

#if !(UNITY_DOTSRUNTIME || (UNITY_2020_1_OR_NEWER && UNITY_IOS))
        static bool _initialized = false;

        [BurstDiscard]
        private static void CheckDelegate(ref bool useDelegate)
        {
            //@TODO: This should use BurstCompiler.IsEnabled once that is available as an efficient API.
            useDelegate = true;
        }

        private static bool UseDelegate()
        {
            bool result = false;
            CheckDelegate(ref result);
            return result;
        }

        private delegate void _dlg_AddComponentEntitiesBatch(EntityComponentStore* entityComponentStore, Unity.Collections.LowLevel.Unsafe.UnsafeList* entityBatchList, int typeIndex);
        private static _dlg_AddComponentEntitiesBatch _bfp_AddComponentEntitiesBatch;
        private delegate bool _dlg_AddComponentEntity(EntityComponentStore* entityComponentStore, Entity* entity, int typeIndex);
        private static _dlg_AddComponentEntity _bfp_AddComponentEntity;
        private delegate void _dlg_AddComponentChunks(EntityComponentStore* entityComponentStore, ArchetypeChunk* chunks, int chunkCount, int typeIndex);
        private static _dlg_AddComponentChunks _bfp_AddComponentChunks;
        private delegate void _dlg_AddComponentsChunks(EntityComponentStore* entityComponentStore, ArchetypeChunk* chunks, int chunkCount, ref ComponentTypes types);
        private static _dlg_AddComponentsChunks _bfp_AddComponentsChunks;
        private delegate bool _dlg_RemoveComponentEntity(EntityComponentStore* entityComponentStore, Entity* entity, int typeIndex);
        private static _dlg_RemoveComponentEntity _bfp_RemoveComponentEntity;
        private delegate void _dlg_RemoveComponentEntitiesBatch(EntityComponentStore* entityComponentStore, Unity.Collections.LowLevel.Unsafe.UnsafeList* entityBatchList, int typeIndex);
        private static _dlg_RemoveComponentEntitiesBatch _bfp_RemoveComponentEntitiesBatch;
        private delegate void _dlg_RemoveComponentChunks(EntityComponentStore* entityComponentStore, ArchetypeChunk* chunks, int chunkCount, int typeIndex);
        private static _dlg_RemoveComponentChunks _bfp_RemoveComponentChunks;
        private delegate void _dlg_RemoveComponentsChunks(EntityComponentStore* entityComponentStore, ArchetypeChunk* chunks, int chunkCount, ref ComponentTypes types);
        private static _dlg_RemoveComponentsChunks _bfp_RemoveComponentsChunks;
        private delegate void _dlg_AddSharedComponentChunks(EntityComponentStore* entityComponentStore, ArchetypeChunk* chunks, int chunkCount, int componentTypeIndex, int sharedComponentIndex);
        private static _dlg_AddSharedComponentChunks _bfp_AddSharedComponentChunks;
        private delegate void _dlg_MoveEntityArchetype(EntityComponentStore* entityComponentStore, Entity* entity, void* dstArchetype);
        private static _dlg_MoveEntityArchetype _bfp_MoveEntityArchetype;
        private delegate void _dlg_SetChunkComponent(EntityComponentStore* entityComponentStore, ArchetypeChunk* chunks, int chunkCount, void* componentData, int componentTypeIndex);
        private static _dlg_SetChunkComponent _bfp_SetChunkComponent;
        private delegate void _dlg_CreateEntity(EntityComponentStore* entityComponentStore, void* archetype, Entity* outEntities, int count);
        private static _dlg_CreateEntity _bfp_CreateEntity;
        private delegate void _dlg_InstantiateEntities(EntityComponentStore* entityComponentStore, Entity* srcEntity, Entity* outputEntities, int instanceCount);
        private static _dlg_InstantiateEntities _bfp_InstantiateEntities;

#endif

        internal static void Initialize()
        {
#if !(UNITY_DOTSRUNTIME || (UNITY_2020_1_OR_NEWER && UNITY_IOS))
            if (_initialized)
                return;
            _initialized = true;
            _bfp_AddComponentEntitiesBatch = BurstCompiler.CompileFunctionPointer<_dlg_AddComponentEntitiesBatch>(_mono_to_burst_AddComponentEntitiesBatch).Invoke;
            _bfp_AddComponentEntity = BurstCompiler.CompileFunctionPointer<_dlg_AddComponentEntity>(_mono_to_burst_AddComponentEntity).Invoke;
            _bfp_AddComponentChunks = BurstCompiler.CompileFunctionPointer<_dlg_AddComponentChunks>(_mono_to_burst_AddComponentChunks).Invoke;
            _bfp_AddComponentsChunks = BurstCompiler.CompileFunctionPointer<_dlg_AddComponentsChunks>(_mono_to_burst_AddComponentsChunks).Invoke;
            _bfp_RemoveComponentEntity = BurstCompiler.CompileFunctionPointer<_dlg_RemoveComponentEntity>(_mono_to_burst_RemoveComponentEntity).Invoke;
            _bfp_RemoveComponentEntitiesBatch = BurstCompiler.CompileFunctionPointer<_dlg_RemoveComponentEntitiesBatch>(_mono_to_burst_RemoveComponentEntitiesBatch).Invoke;
            _bfp_RemoveComponentChunks = BurstCompiler.CompileFunctionPointer<_dlg_RemoveComponentChunks>(_mono_to_burst_RemoveComponentChunks).Invoke;
            _bfp_RemoveComponentsChunks = BurstCompiler.CompileFunctionPointer<_dlg_RemoveComponentsChunks>(_mono_to_burst_RemoveComponentsChunks).Invoke;
            _bfp_AddSharedComponentChunks = BurstCompiler.CompileFunctionPointer<_dlg_AddSharedComponentChunks>(_mono_to_burst_AddSharedComponentChunks).Invoke;
            _bfp_MoveEntityArchetype = BurstCompiler.CompileFunctionPointer<_dlg_MoveEntityArchetype>(_mono_to_burst_MoveEntityArchetype).Invoke;
            _bfp_SetChunkComponent = BurstCompiler.CompileFunctionPointer<_dlg_SetChunkComponent>(_mono_to_burst_SetChunkComponent).Invoke;
            _bfp_CreateEntity = BurstCompiler.CompileFunctionPointer<_dlg_CreateEntity>(_mono_to_burst_CreateEntity).Invoke;
            _bfp_InstantiateEntities = BurstCompiler.CompileFunctionPointer<_dlg_InstantiateEntities>(_mono_to_burst_InstantiateEntities).Invoke;

#endif
        }

        public  static void AddComponentEntitiesBatch (EntityComponentStore* entityComponentStore, Unity.Collections.LowLevel.Unsafe.UnsafeList* entityBatchList, int typeIndex)
        {
#if !(UNITY_DOTSRUNTIME || (UNITY_2020_1_OR_NEWER && UNITY_IOS))
            if (UseDelegate())
            {
                _forward_mono_AddComponentEntitiesBatch(entityComponentStore, entityBatchList, typeIndex);
                return;
            }
#endif

            _AddComponentEntitiesBatch(entityComponentStore, entityBatchList, typeIndex);
        }

#if !(UNITY_DOTSRUNTIME || (UNITY_2020_1_OR_NEWER && UNITY_IOS))
        [BurstCompile]
        [MonoPInvokeCallback(typeof(_dlg_AddComponentEntitiesBatch))]
        private static void _mono_to_burst_AddComponentEntitiesBatch(EntityComponentStore* entityComponentStore, Unity.Collections.LowLevel.Unsafe.UnsafeList* entityBatchList, int typeIndex)
        {
            _AddComponentEntitiesBatch(entityComponentStore, entityBatchList, typeIndex);
        }

        [BurstDiscard]
        private static void _forward_mono_AddComponentEntitiesBatch(EntityComponentStore* entityComponentStore, Unity.Collections.LowLevel.Unsafe.UnsafeList* entityBatchList, int typeIndex)
        {
            _bfp_AddComponentEntitiesBatch(entityComponentStore, entityBatchList, typeIndex);
        }
#endif

        public  static bool AddComponentEntity (EntityComponentStore* entityComponentStore, Entity* entity, int typeIndex)
        {
#if !(UNITY_DOTSRUNTIME || (UNITY_2020_1_OR_NEWER && UNITY_IOS))
            if (UseDelegate())
            {
                var _retval = default(bool);
                _forward_mono_AddComponentEntity(ref _retval, entityComponentStore, entity, typeIndex);
                return _retval;
            }
#endif

            return _AddComponentEntity(entityComponentStore, entity, typeIndex);
        }

#if !(UNITY_DOTSRUNTIME || (UNITY_2020_1_OR_NEWER && UNITY_IOS))
        [BurstCompile]
        [MonoPInvokeCallback(typeof(_dlg_AddComponentEntity))]
        private static bool _mono_to_burst_AddComponentEntity(EntityComponentStore* entityComponentStore, Entity* entity, int typeIndex)
        {
            return _AddComponentEntity(entityComponentStore, entity, typeIndex);
        }

        [BurstDiscard]
        private static void _forward_mono_AddComponentEntity(ref bool _retval, EntityComponentStore* entityComponentStore, Entity* entity, int typeIndex)
        {
            _retval = _bfp_AddComponentEntity(entityComponentStore, entity, typeIndex);
        }
#endif

        public  static void AddComponentChunks (EntityComponentStore* entityComponentStore, ArchetypeChunk* chunks, int chunkCount, int typeIndex)
        {
#if !(UNITY_DOTSRUNTIME || (UNITY_2020_1_OR_NEWER && UNITY_IOS))
            if (UseDelegate())
            {
                _forward_mono_AddComponentChunks(entityComponentStore, chunks, chunkCount, typeIndex);
                return;
            }
#endif

            _AddComponentChunks(entityComponentStore, chunks, chunkCount, typeIndex);
        }

#if !(UNITY_DOTSRUNTIME || (UNITY_2020_1_OR_NEWER && UNITY_IOS))
        [BurstCompile]
        [MonoPInvokeCallback(typeof(_dlg_AddComponentChunks))]
        private static void _mono_to_burst_AddComponentChunks(EntityComponentStore* entityComponentStore, ArchetypeChunk* chunks, int chunkCount, int typeIndex)
        {
            _AddComponentChunks(entityComponentStore, chunks, chunkCount, typeIndex);
        }

        [BurstDiscard]
        private static void _forward_mono_AddComponentChunks(EntityComponentStore* entityComponentStore, ArchetypeChunk* chunks, int chunkCount, int typeIndex)
        {
            _bfp_AddComponentChunks(entityComponentStore, chunks, chunkCount, typeIndex);
        }
#endif

        public  static void AddComponentsChunks (EntityComponentStore* entityComponentStore, ArchetypeChunk* chunks, int chunkCount, ref ComponentTypes types)
        {
#if !(UNITY_DOTSRUNTIME || (UNITY_2020_1_OR_NEWER && UNITY_IOS))
            if (UseDelegate())
            {
                _forward_mono_AddComponentsChunks(entityComponentStore, chunks, chunkCount, ref types);
                return;
            }
#endif

            _AddComponentsChunks(entityComponentStore, chunks, chunkCount, ref types);
        }

#if !(UNITY_DOTSRUNTIME || (UNITY_2020_1_OR_NEWER && UNITY_IOS))
        [BurstCompile]
        [MonoPInvokeCallback(typeof(_dlg_AddComponentsChunks))]
        private static void _mono_to_burst_AddComponentsChunks(EntityComponentStore* entityComponentStore, ArchetypeChunk* chunks, int chunkCount, ref ComponentTypes types)
        {
            _AddComponentsChunks(entityComponentStore, chunks, chunkCount, ref types);
        }

        [BurstDiscard]
        private static void _forward_mono_AddComponentsChunks(EntityComponentStore* entityComponentStore, ArchetypeChunk* chunks, int chunkCount, ref ComponentTypes types)
        {
            _bfp_AddComponentsChunks(entityComponentStore, chunks, chunkCount, ref types);
        }
#endif

        public  static bool RemoveComponentEntity (EntityComponentStore* entityComponentStore, Entity* entity, int typeIndex)
        {
#if !(UNITY_DOTSRUNTIME || (UNITY_2020_1_OR_NEWER && UNITY_IOS))
            if (UseDelegate())
            {
                var _retval = default(bool);
                _forward_mono_RemoveComponentEntity(ref _retval, entityComponentStore, entity, typeIndex);
                return _retval;
            }
#endif

            return _RemoveComponentEntity(entityComponentStore, entity, typeIndex);
        }

#if !(UNITY_DOTSRUNTIME || (UNITY_2020_1_OR_NEWER && UNITY_IOS))
        [BurstCompile]
        [MonoPInvokeCallback(typeof(_dlg_RemoveComponentEntity))]
        private static bool _mono_to_burst_RemoveComponentEntity(EntityComponentStore* entityComponentStore, Entity* entity, int typeIndex)
        {
            return _RemoveComponentEntity(entityComponentStore, entity, typeIndex);
        }

        [BurstDiscard]
        private static void _forward_mono_RemoveComponentEntity(ref bool _retval, EntityComponentStore* entityComponentStore, Entity* entity, int typeIndex)
        {
            _retval = _bfp_RemoveComponentEntity(entityComponentStore, entity, typeIndex);
        }
#endif

        public  static void RemoveComponentEntitiesBatch (EntityComponentStore* entityComponentStore, Unity.Collections.LowLevel.Unsafe.UnsafeList* entityBatchList, int typeIndex)
        {
#if !(UNITY_DOTSRUNTIME || (UNITY_2020_1_OR_NEWER && UNITY_IOS))
            if (UseDelegate())
            {
                _forward_mono_RemoveComponentEntitiesBatch(entityComponentStore, entityBatchList, typeIndex);
                return;
            }
#endif

            _RemoveComponentEntitiesBatch(entityComponentStore, entityBatchList, typeIndex);
        }

#if !(UNITY_DOTSRUNTIME || (UNITY_2020_1_OR_NEWER && UNITY_IOS))
        [BurstCompile]
        [MonoPInvokeCallback(typeof(_dlg_RemoveComponentEntitiesBatch))]
        private static void _mono_to_burst_RemoveComponentEntitiesBatch(EntityComponentStore* entityComponentStore, Unity.Collections.LowLevel.Unsafe.UnsafeList* entityBatchList, int typeIndex)
        {
            _RemoveComponentEntitiesBatch(entityComponentStore, entityBatchList, typeIndex);
        }

        [BurstDiscard]
        private static void _forward_mono_RemoveComponentEntitiesBatch(EntityComponentStore* entityComponentStore, Unity.Collections.LowLevel.Unsafe.UnsafeList* entityBatchList, int typeIndex)
        {
            _bfp_RemoveComponentEntitiesBatch(entityComponentStore, entityBatchList, typeIndex);
        }
#endif

        public  static void RemoveComponentChunks (EntityComponentStore* entityComponentStore, ArchetypeChunk* chunks, int chunkCount, int typeIndex)
        {
#if !(UNITY_DOTSRUNTIME || (UNITY_2020_1_OR_NEWER && UNITY_IOS))
            if (UseDelegate())
            {
                _forward_mono_RemoveComponentChunks(entityComponentStore, chunks, chunkCount, typeIndex);
                return;
            }
#endif

            _RemoveComponentChunks(entityComponentStore, chunks, chunkCount, typeIndex);
        }

#if !(UNITY_DOTSRUNTIME || (UNITY_2020_1_OR_NEWER && UNITY_IOS))
        [BurstCompile]
        [MonoPInvokeCallback(typeof(_dlg_RemoveComponentChunks))]
        private static void _mono_to_burst_RemoveComponentChunks(EntityComponentStore* entityComponentStore, ArchetypeChunk* chunks, int chunkCount, int typeIndex)
        {
            _RemoveComponentChunks(entityComponentStore, chunks, chunkCount, typeIndex);
        }

        [BurstDiscard]
        private static void _forward_mono_RemoveComponentChunks(EntityComponentStore* entityComponentStore, ArchetypeChunk* chunks, int chunkCount, int typeIndex)
        {
            _bfp_RemoveComponentChunks(entityComponentStore, chunks, chunkCount, typeIndex);
        }
#endif

        public  static void RemoveComponentsChunks (EntityComponentStore* entityComponentStore, ArchetypeChunk* chunks, int chunkCount, ref ComponentTypes types)
        {
#if !(UNITY_DOTSRUNTIME || (UNITY_2020_1_OR_NEWER && UNITY_IOS))
            if (UseDelegate())
            {
                _forward_mono_RemoveComponentsChunks(entityComponentStore, chunks, chunkCount, ref types);
                return;
            }
#endif

            _RemoveComponentsChunks(entityComponentStore, chunks, chunkCount, ref types);
        }

#if !(UNITY_DOTSRUNTIME || (UNITY_2020_1_OR_NEWER && UNITY_IOS))
        [BurstCompile]
        [MonoPInvokeCallback(typeof(_dlg_RemoveComponentsChunks))]
        private static void _mono_to_burst_RemoveComponentsChunks(EntityComponentStore* entityComponentStore, ArchetypeChunk* chunks, int chunkCount, ref ComponentTypes types)
        {
            _RemoveComponentsChunks(entityComponentStore, chunks, chunkCount, ref types);
        }

        [BurstDiscard]
        private static void _forward_mono_RemoveComponentsChunks(EntityComponentStore* entityComponentStore, ArchetypeChunk* chunks, int chunkCount, ref ComponentTypes types)
        {
            _bfp_RemoveComponentsChunks(entityComponentStore, chunks, chunkCount, ref types);
        }
#endif

        public  static void AddSharedComponentChunks (EntityComponentStore* entityComponentStore, ArchetypeChunk* chunks, int chunkCount, int componentTypeIndex, int sharedComponentIndex)
        {
#if !(UNITY_DOTSRUNTIME || (UNITY_2020_1_OR_NEWER && UNITY_IOS))
            if (UseDelegate())
            {
                _forward_mono_AddSharedComponentChunks(entityComponentStore, chunks, chunkCount, componentTypeIndex, sharedComponentIndex);
                return;
            }
#endif

            _AddSharedComponentChunks(entityComponentStore, chunks, chunkCount, componentTypeIndex, sharedComponentIndex);
        }

#if !(UNITY_DOTSRUNTIME || (UNITY_2020_1_OR_NEWER && UNITY_IOS))
        [BurstCompile]
        [MonoPInvokeCallback(typeof(_dlg_AddSharedComponentChunks))]
        private static void _mono_to_burst_AddSharedComponentChunks(EntityComponentStore* entityComponentStore, ArchetypeChunk* chunks, int chunkCount, int componentTypeIndex, int sharedComponentIndex)
        {
            _AddSharedComponentChunks(entityComponentStore, chunks, chunkCount, componentTypeIndex, sharedComponentIndex);
        }

        [BurstDiscard]
        private static void _forward_mono_AddSharedComponentChunks(EntityComponentStore* entityComponentStore, ArchetypeChunk* chunks, int chunkCount, int componentTypeIndex, int sharedComponentIndex)
        {
            _bfp_AddSharedComponentChunks(entityComponentStore, chunks, chunkCount, componentTypeIndex, sharedComponentIndex);
        }
#endif

        public  static void MoveEntityArchetype (EntityComponentStore* entityComponentStore, Entity* entity, void* dstArchetype)
        {
#if !(UNITY_DOTSRUNTIME || (UNITY_2020_1_OR_NEWER && UNITY_IOS))
            if (UseDelegate())
            {
                _forward_mono_MoveEntityArchetype(entityComponentStore, entity, dstArchetype);
                return;
            }
#endif

            _MoveEntityArchetype(entityComponentStore, entity, dstArchetype);
        }

#if !(UNITY_DOTSRUNTIME || (UNITY_2020_1_OR_NEWER && UNITY_IOS))
        [BurstCompile]
        [MonoPInvokeCallback(typeof(_dlg_MoveEntityArchetype))]
        private static void _mono_to_burst_MoveEntityArchetype(EntityComponentStore* entityComponentStore, Entity* entity, void* dstArchetype)
        {
            _MoveEntityArchetype(entityComponentStore, entity, dstArchetype);
        }

        [BurstDiscard]
        private static void _forward_mono_MoveEntityArchetype(EntityComponentStore* entityComponentStore, Entity* entity, void* dstArchetype)
        {
            _bfp_MoveEntityArchetype(entityComponentStore, entity, dstArchetype);
        }
#endif

        public  static void SetChunkComponent (EntityComponentStore* entityComponentStore, ArchetypeChunk* chunks, int chunkCount, void* componentData, int componentTypeIndex)
        {
#if !(UNITY_DOTSRUNTIME || (UNITY_2020_1_OR_NEWER && UNITY_IOS))
            if (UseDelegate())
            {
                _forward_mono_SetChunkComponent(entityComponentStore, chunks, chunkCount, componentData, componentTypeIndex);
                return;
            }
#endif

            _SetChunkComponent(entityComponentStore, chunks, chunkCount, componentData, componentTypeIndex);
        }

#if !(UNITY_DOTSRUNTIME || (UNITY_2020_1_OR_NEWER && UNITY_IOS))
        [BurstCompile]
        [MonoPInvokeCallback(typeof(_dlg_SetChunkComponent))]
        private static void _mono_to_burst_SetChunkComponent(EntityComponentStore* entityComponentStore, ArchetypeChunk* chunks, int chunkCount, void* componentData, int componentTypeIndex)
        {
            _SetChunkComponent(entityComponentStore, chunks, chunkCount, componentData, componentTypeIndex);
        }

        [BurstDiscard]
        private static void _forward_mono_SetChunkComponent(EntityComponentStore* entityComponentStore, ArchetypeChunk* chunks, int chunkCount, void* componentData, int componentTypeIndex)
        {
            _bfp_SetChunkComponent(entityComponentStore, chunks, chunkCount, componentData, componentTypeIndex);
        }
#endif

        public  static void CreateEntity (EntityComponentStore* entityComponentStore, void* archetype, Entity* outEntities, int count)
        {
#if !(UNITY_DOTSRUNTIME || (UNITY_2020_1_OR_NEWER && UNITY_IOS))
            if (UseDelegate())
            {
                _forward_mono_CreateEntity(entityComponentStore, archetype, outEntities, count);
                return;
            }
#endif

            _CreateEntity(entityComponentStore, archetype, outEntities, count);
        }

#if !(UNITY_DOTSRUNTIME || (UNITY_2020_1_OR_NEWER && UNITY_IOS))
        [BurstCompile]
        [MonoPInvokeCallback(typeof(_dlg_CreateEntity))]
        private static void _mono_to_burst_CreateEntity(EntityComponentStore* entityComponentStore, void* archetype, Entity* outEntities, int count)
        {
            _CreateEntity(entityComponentStore, archetype, outEntities, count);
        }

        [BurstDiscard]
        private static void _forward_mono_CreateEntity(EntityComponentStore* entityComponentStore, void* archetype, Entity* outEntities, int count)
        {
            _bfp_CreateEntity(entityComponentStore, archetype, outEntities, count);
        }
#endif

        public  static void InstantiateEntities (EntityComponentStore* entityComponentStore, Entity* srcEntity, Entity* outputEntities, int instanceCount)
        {
#if !(UNITY_DOTSRUNTIME || (UNITY_2020_1_OR_NEWER && UNITY_IOS))
            if (UseDelegate())
            {
                _forward_mono_InstantiateEntities(entityComponentStore, srcEntity, outputEntities, instanceCount);
                return;
            }
#endif

            _InstantiateEntities(entityComponentStore, srcEntity, outputEntities, instanceCount);
        }

#if !(UNITY_DOTSRUNTIME || (UNITY_2020_1_OR_NEWER && UNITY_IOS))
        [BurstCompile]
        [MonoPInvokeCallback(typeof(_dlg_InstantiateEntities))]
        private static void _mono_to_burst_InstantiateEntities(EntityComponentStore* entityComponentStore, Entity* srcEntity, Entity* outputEntities, int instanceCount)
        {
            _InstantiateEntities(entityComponentStore, srcEntity, outputEntities, instanceCount);
        }

        [BurstDiscard]
        private static void _forward_mono_InstantiateEntities(EntityComponentStore* entityComponentStore, Entity* srcEntity, Entity* outputEntities, int instanceCount)
        {
            _bfp_InstantiateEntities(entityComponentStore, srcEntity, outputEntities, instanceCount);
        }
#endif




    }
}
