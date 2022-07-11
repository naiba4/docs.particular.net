---
title: Externalize Windows Performance Counters
reviewed: 2020-11-09
component: PerfCounters
isUpgradeGuide: true
upgradeGuideCoreVersions:
 - 6
related: 
 - monitoring/metrics/performance-counters
---

include: externalize-perfcounters

## Changed APIs

The NServiceBus Performance Counter APIs have been marked as obsolete and have one-for-one equivalents in the new `NServiceBus.Metrics.PerformanceCounters` package.

### Enabling Critical Time Counter

In 1.x 
```csharp
var performanceCounters = endpointConfiguration.EnableWindowsPerformanceCounters();
```

In 6.x 
```csharp
endpointConfiguration.EnableCriticalTimePerformanceCounter();
```

### Enabling SLA Counter

In 1.x
```csharp
var performanceCounters = endpointConfiguration.EnableWindowsPerformanceCounters();
performanceCounters.EnableSLAPerformanceCounters(TimeSpan.FromMinutes(3));
```

In 6.x 
```csharp 
endpointConfiguration.EnableSLAPerformanceCounter(TimeSpan.FromMinutes(3));
```

## Compatibility

The `NServiceBus.Metrics.PerformanceCounters` package is fully compatible with endpoints that use the NServiceBus package's Performance Counters functionality.
