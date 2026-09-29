using Komari.Domain.Enums;

namespace Komari.Application.Tables.DTOs;

public record TableResponse(
    Guid Id,
    int Number,
    int Capacity,
    TableType Type,
    TableStatus Status,
    string? Location,
    bool IsActive,
    DateTime CreatedAt
);
