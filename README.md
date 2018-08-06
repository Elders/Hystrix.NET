![Hystrix](https://camo.githubusercontent.com/e871b5d002a9699e7a2d9fa0178af5c72f0743e0/68747470733a2f2f6e6574666c69782e6769746875622e636f6d2f487973747269782f696d616765732f687973747269782d6c6f676f2d7461676c696e652d3835302e706e67 "Hystrix")

Hystrix.NET is a C# port of Hystrix, which is a latency and fault tolerance library for complex distributed systems.

[Hystrix Wiki](https://github.com/Netflix/Hystrix/wiki)  
[Hystrix on GitHub](https://github.com/Netflix/Hystrix)

# What is Hystrix?
In a distributed environment, failure of any given service is inevitable. Hystrix is a library designed to control the interactions between these distributed services providing greater latency and fault tolerance. Hystrix does this by isolating points of access between the services, stopping cascading failures across them, and providing fallback options, all of which improve the system's overall resiliency.

Hystrix evolved out of resilience engineering work that the Netflix API team began in 2011. Over the course of 2012, Hystrix continued to evolve and mature, eventually leading to adoption across many teams within Netflix. Today tens of billions of thread-isolated and hundreds of billions of semaphore-isolated calls are executed via Hystrix every day at Netflix and a dramatic improvement in uptime and resilience has been achieved through its use.

