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

    public PhysicsWorld(int capacity = 10_000)
    {
        Capacity = capacity;

        nuint floatBytes = (nuint)(capacity * sizeof(float));
        nuint byteBytes = (nuint)(capacity * sizeof(byte));

        _posX = (float*)NativeMemory.AlignedAlloc(floatBytes, 32);
        _posZ = (float*)NativeMemory.AlignedAlloc(floatBytes, 32);
        _velocityX = (float*)NativeMemory.AlignedAlloc(floatBytes, 32);
        _velocityZ = (float*)NativeMemory.AlignedAlloc(floatBytes, 32);
        _radius = (float*)NativeMemory.AlignedAlloc(floatBytes, 32);

        _types = (BodyType*)NativeMemory.AlignedAlloc(byteBytes, 32);
        _isActive = (bool*)NativeMemory.AlignedAlloc(byteBytes, 32);

    }

    public int RegisterBody(float x = 0f, float z = 0f, float radius = 1f, BodyType type = BodyType.Dynamic)
    {
        
        if (Count >= Capacity) {Console.WriteLine("[PhysicsWorld] No free space, body not added."); return -1;};

        int handle = Count;

        _posX[handle] = x;
        _posZ[handle] = z;
        _velocityX[handle] = 0f;
        _velocityZ[handle] = 0f;
        _radius[handle] = radius;
        _types[handle] = type;
        _isActive[handle] = true;

        Count++;

        return handle;


    }

    public void SetVelocity(int handle, float vx = 0f, float vz = 0)
    {
        
        if (handle < 0 || handle >= Capacity || !_isActive[handle]) return;

        _velocityX[handle] = vx;
        _velocityZ[handle] = vz;

    }


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

        GC.SuppressFinalize(this);
        _disposed = true;

    }
    ~PhysicsWorld()
    {
        Dispose();
    }


}