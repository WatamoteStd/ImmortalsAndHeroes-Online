using System.Diagnostics;
using PhysWorld.Core;

PhysicsWorld physicsWorld = new PhysicsWorld(100_000);

float min = -1000f;
float max = 1000f;

for (int i = 0; i < 100000; i++)
{
    
    float randPosX = min + Random.Shared.NextSingle() * (max - min);
    float randPosZ = min + Random.Shared.NextSingle() * (max - min);

    float randVelX = (Random.Shared.NextSingle() - 0.5f) * 1000f;
    float randVelZ = (Random.Shared.NextSingle() - 0.5f) * 1000f;

    int handle = physicsWorld.RegisterBody(randPosX, randPosZ);
    physicsWorld.SetVelocity(handle, randVelX, randVelZ);

}

for (int i = 0; i < 10000; i++)
{
    physicsWorld.Step(0.016f);
}

Stopwatch sw = Stopwatch.StartNew();
for(int i = 0; i < 100_000; i++)
{
    physicsWorld.Step(0.016f);
}
sw.Stop();
Console.WriteLine($"Total Time:{sw.Elapsed.TotalMilliseconds}ms");
double avgMicroseconds = (sw.Elapsed.TotalMilliseconds / 100_000) * 1000;
Console.WriteLine($"1 Step avg: {avgMicroseconds:F2} microseconds");
