using Komari.Domain.Enums;

namespace Komari.Application.Tables.DTOs;

public record CreateTableRequest(
    int Number,
    int Capacity,
    TableType Type,
    string? Location = null
);
