namespace CaeDashboard.EngineeringRequests.Models;

public enum RequestPriority
{
    P1 = 1,
    P2 = 2,
    P3 = 3,
    P4 = 4
}

public enum RequestStatus
{
    Incoming = 1,
    Planned = 2,
    InProgress = 3,
    Waiting = 4,
    Done = 5
}

public enum RequestType
{
    Bug = 1,
    Feature = 2,
    Support = 3,
    Validation = 4,
    Investigation = 5,
    TechnicalDebt = 6
}

public enum SystemCriticality
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}
