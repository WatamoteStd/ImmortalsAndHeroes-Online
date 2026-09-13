using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

namespace PhysWorld.Core;

public unsafe class PhysicsWorld : IDisposable
{
    
    public enum BodyType : byte { Static, Dynamic, Trigger }
    private float* _posX, _posZ, _velocityX, _velocityZ, _radius;
    private BodyType* _types;
    private bool* _isActive;
    public int Capacity {get; private set;}
    public int Count {get; private set;}
    private bool _disposed = false;


    private int* _denseToEntityId;
    private int* _entityToDenseId;
    private Stack<int> _nextEntityId;

    public PhysicsWorld(int capacity = 10_000)
    {
        Capacity = capacity;

        nuint floatBytes = (nuint)(capacity * sizeof(float));
        nuint byteBytes = (nuint)(capacity * sizeof(byte));
        nuint intBytes = (nuint)(capacity * sizeof(int));

        _posX = (float*)NativeMemory.AlignedAlloc(floatBytes, 32);
        _posZ = (float*)NativeMemory.AlignedAlloc(floatBytes, 32);
        _velocityX = (float*)NativeMemory.AlignedAlloc(floatBytes, 32);
        _velocityZ = (float*)NativeMemory.AlignedAlloc(floatBytes, 32);
        _radius = (float*)NativeMemory.AlignedAlloc(floatBytes, 32);

        _types = (BodyType*)NativeMemory.AlignedAlloc(byteBytes, 32);
        _isActive = (bool*)NativeMemory.AlignedAlloc(byteBytes, 32);

        _denseToEntityId = (int*)NativeMemory.AlignedAlloc(intBytes, 32);
        _entityToDenseId = (int*)NativeMemory.AlignedAlloc(intBytes, 32);

        _nextEntityId = new Stack<int>();
        for (int i = capacity - 1; i >= 0; i--)
        {
            _nextEntityId.Push(i);
        }


    }

    public int RegisterBody(float x = 0f, float z = 0f, float radius = 1f, BodyType type = BodyType.Dynamic)
    {
        
        if (Count >= Capacity) {Console.WriteLine("[PhysicsWorld] No free space, body not added."); return -1;};

        int internalId = Count;
        int entityId = _nextEntityId.Pop();

        _entityToDenseId[entityId] = internalId;
        _denseToEntityId[internalId] = entityId;

        _posX[internalId] = x;
        _posZ[internalId] = z;
        _velocityX[internalId] = 0f;
        _velocityZ[internalId] = 0f;
        _radius[internalId] = radius;
        _types[internalId] = type;
        _isActive[internalId] = true;

        Count++;

        return entityId;


    }

    /// <summary>
    /// Change velocity of the selected entity.
    /// </summary>
    /// <param name="entityId">EntityId! Not InternalId</param>
    public void SetVelocity(int entityId, float vx = 0f, float vz = 0)
    {
        if (entityId < 0 || entityId >= Capacity) return;
        int internId = _entityToDenseId[entityId];
        if (internId < 0 || internId >= Count || !_isActive[internId]) return;

        _velocityX[internId] = vx;
        _velocityZ[internId] = vz;

    }

    /// <summary>
    /// Makes one step in physics simulation.
    /// </summary>
    /// <param name="deltaTime">Frame time in seconds (e.g. 0.016f)</param>
    /// <remarks>
    /// Uses SIMD AVX2 acceleration over dense SoA arrays.
    /// </remarks>
    public void Step(float deltaTime)
    {
        
        Vector256<float> dtVec = Vector256.Create(deltaTime);
        int vectorizedCount = Count - (Count % 8);

        for (int i = 0; i < vectorizedCount; i += 8)
        {
            
            //                X
            var posX = Vector256.Load(_posX + i);
            var velX = Vector256.Load(_velocityX + i);
            Vector256.Store(posX + (velX * dtVec), _posX + i);

            //                Z
            var posZ = Vector256.Load(_posZ + i);
            var velZ = Vector256.Load(_velocityZ + i);
            Vector256.Store(posZ + (velZ * dtVec), _posZ + i);


        }

        for (int i = vectorizedCount; i < Count; i++)
        {
            _posX[i] += _velocityX[i] * deltaTime;
            _posZ[i] += _velocityZ[i] * deltaTime;
        }

    }



    public void UnregisterBody(int entityId)
    {
        
        if (entityId < 0 || entityId >= Capacity) return;
        int internId = _entityToDenseId[entityId];
        if (internId < 0 || internId >= Count) return;

        int lastInternal = Count - 1;
        int lastEntityId = _denseToEntityId[lastInternal];

        if (internId != lastInternal)
        {
            
             _posX[internId] = _posX[lastInternal];
            _posZ[internId] = _posZ[lastInternal];
            _velocityX[internId] = _velocityX[lastInternal];
            _velocityZ[internId] = _velocityZ[lastInternal];
            _radius[internId] = _radius[lastInternal];

            _types[internId] = _types[lastInternal];
            _isActive[internId] = _isActive[lastInternal];

            _denseToEntityId[internId] = lastEntityId;
            _entityToDenseId[lastEntityId] = internId;

        }

        _entityToDenseId[entityId] = -1;
        _nextEntityId.Push(entityId);
        Count--;

        

    }




    public void Dispose()
    {
        
        if (_disposed) return;

        NativeMemory.AlignedFree(_posX);
        NativeMemory.AlignedFree(_posZ);
        NativeMemory.AlignedFree(_velocityX);
        NativeMemory.AlignedFree(_velocityZ);
        NativeMemory.AlignedFree(_radius);
        NativeMemory.AlignedFree(_types);
        NativeMemory.AlignedFree(_isActive);
        NativeMemory.AlignedFree(_denseToEntityId);
        NativeMemory.AlignedFree(_entityToDenseId);

        GC.SuppressFinalize(this);
        _disposed = true;

    }
    ~PhysicsWorld()
    {
        Dispose();
    }


}