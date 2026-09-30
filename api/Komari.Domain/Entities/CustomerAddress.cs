using Komari.Domain.Common;

namespace Komari.Domain.Entities;

/// <summary>
/// Representa um endereço de entrega vinculado a um cliente (ex: Residência, Trabalho).
/// </summary>
public class CustomerAddress : BaseEntity
{
    public Guid CustomerId { get; private set; }
    public Customer? Customer { get; private set; }

    public string Street { get; private set; } = string.Empty;
    public string Number { get; private set; } = string.Empty;
    public string Neighborhood { get; private set; } = string.Empty;
    public string? ZipCode { get; private set; }
    public string? Complement { get; private set; }
    public string? ReferencePoint { get; private set; }
    public bool IsDefault { get; private set; }

    // Construtor protegido exigido pelo EF Core
    protected CustomerAddress() { }

    public CustomerAddress(
        Guid customerId,
        string street,
        string number,
        string neighborhood,
        string? zipCode = null,
        string? complement = null,
        string? referencePoint = null,
        bool isDefault = false)
    {
        SetCustomerId(customerId);
        SetStreet(street);
        SetNumber(number);
        SetNeighborhood(neighborhood);
        SetZipCode(zipCode);
        Complement = complement?.Trim();
        ReferencePoint = referencePoint?.Trim();
        IsDefault = isDefault;
    }

    public void AttachToCustomer(Guid customerId)
    {
        SetCustomerId(customerId);
    }

    public void UpdateDetails(
        string street,
        string number,
        string neighborhood,
        string? zipCode,
        string? complement,
        string? referencePoint,
        bool isDefault)
    {
        SetStreet(street);
        SetNumber(number);
        SetNeighborhood(neighborhood);
        SetZipCode(zipCode);
        Complement = complement?.Trim();
        ReferencePoint = referencePoint?.Trim();
        IsDefault = isDefault;
        TouchUpdated();
    }

    public void SetDefault(bool isDefault)
    {
        IsDefault = isDefault;
        TouchUpdated();
    }

    public void Deactivate()
    {
        IsActive = false;
        IsDefault = false;
        TouchUpdated();
    }

    private void SetCustomerId(Guid customerId)
    {
        if (customerId == Guid.Empty)
        {
            throw new ArgumentException("O identificador do cliente (CustomerId) não pode ser vazio.", nameof(customerId));
        }
        CustomerId = customerId;
    }

    private void SetStreet(string street)
    {
        if (string.IsNullOrWhiteSpace(street))
        {
            throw new ArgumentException("O logradouro/rua é obrigatório.", nameof(street));
        }

        var trimmed = street.Trim();
        if (trimmed.Length < 2 || trimmed.Length > 200)
        {
            throw new ArgumentOutOfRangeException(nameof(street), "O logradouro deve ter entre 2 e 200 caracteres.");
        }

        Street = trimmed;
    }

    private void SetNumber(string number)
    {
        if (string.IsNullOrWhiteSpace(number))
        {
            throw new ArgumentException("O número do endereço é obrigatório (informe 'S/N' se não houver).", nameof(number));
        }

        var trimmed = number.Trim();
        if (trimmed.Length > 20)
        {
            throw new ArgumentOutOfRangeException(nameof(number), "O número não pode exceder 20 caracteres.");
        }

        Number = trimmed;
    }

    private void SetNeighborhood(string neighborhood)
    {
        if (string.IsNullOrWhiteSpace(neighborhood))
        {
            throw new ArgumentException("O bairro é obrigatório.", nameof(neighborhood));
        }

        var trimmed = neighborhood.Trim();
        if (trimmed.Length < 2 || trimmed.Length > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(neighborhood), "O bairro deve ter entre 2 e 100 caracteres.");
        }

        Neighborhood = trimmed;
    }

    private void SetZipCode(string? zipCode)
    {
        if (string.IsNullOrWhiteSpace(zipCode))
        {
            ZipCode = null;
            return;
        }

        var trimmed = zipCode.Trim();
        if (trimmed.Length > 10)
        {
            throw new ArgumentOutOfRangeException(nameof(zipCode), "O CEP não pode exceder 10 caracteres.");
        }

        ZipCode = trimmed;
    }
}
