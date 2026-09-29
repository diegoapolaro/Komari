using Komari.Domain.Enums;

namespace Komari.Application.Tables.DTOs;

public record UpdateTableRequest(
    int Number,
    int Capacity,
    TableType Type,
    string? Location = null
);
