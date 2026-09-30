using Komari.Domain.Common;

namespace Komari.Domain.Entities;

/// <summary>
/// Representa um cliente cadastrado no restaurante para atendimento em salão, balcão ou delivery.
/// </summary>
public class Customer : BaseEntity
{
    private readonly List<CustomerAddress> _addresses = new();

    public string Name { get; private set; } = string.Empty;
    public string? Phone { get; private set; }
    public string? Email { get; private set; }
    public string? Document { get; private set; }
    public string? Notes { get; private set; }

    public IReadOnlyCollection<CustomerAddress> Addresses => _addresses.AsReadOnly();

    // Construtor protegido exigido pelo EF Core
    protected Customer() { }

    public Customer(
        string name,
        string? phone = null,
        string? email = null,
        string? document = null,
        string? notes = null)
    {
        SetName(name);
        SetPhone(phone);
        SetEmail(email);
        SetDocument(document);
        Notes = notes?.Trim();
    }

    public void UpdateDetails(
        string name,
        string? phone,
        string? email,
        string? document,
        string? notes)
    {
        SetName(name);
        SetPhone(phone);
        SetEmail(email);
        SetDocument(document);
        Notes = notes?.Trim();
        TouchUpdated();
    }

    public CustomerAddress AddAddress(
        string street,
        string number,
        string neighborhood,
        string? zipCode = null,
        string? complement = null,
        string? referencePoint = null,
        bool isDefault = false)
    {
        var activeAddresses = _addresses.Where(a => a.IsActive).ToList();
        var shouldBeDefault = isDefault || !activeAddresses.Any();

        if (shouldBeDefault)
        {
            foreach (var addr in activeAddresses)
            {
                addr.SetDefault(false);
            }
        }

        var address = new CustomerAddress(
            Id,
            street,
            number,
            neighborhood,
            zipCode,
            complement,
            referencePoint,
            shouldBeDefault);

        _addresses.Add(address);
        TouchUpdated();

        return address;
    }

    public void UpdateAddress(
        Guid addressId,
        string street,
        string number,
        string neighborhood,
        string? zipCode,
        string? complement,
        string? referencePoint,
        bool isDefault)
    {
        var address = _addresses.FirstOrDefault(a => a.Id == addressId && a.IsActive);
        if (address == null)
        {
            throw new InvalidOperationException($"Endereço com identificador '{addressId}' não encontrado ou inativo.");
        }

        if (isDefault)
        {
            foreach (var addr in _addresses.Where(a => a.IsActive && a.Id != addressId))
            {
                addr.SetDefault(false);
            }
        }
        else if (address.IsDefault)
        {
            var anotherAddress = _addresses.FirstOrDefault(a => a.IsActive && a.Id != addressId);
            if (anotherAddress != null)
            {
                anotherAddress.SetDefault(true);
            }
            else
            {
                isDefault = true;
            }
        }

        address.UpdateDetails(street, number, neighborhood, zipCode, complement, referencePoint, isDefault);
        TouchUpdated();
    }

    public void SetDefaultAddress(Guid addressId)
    {
        var target = _addresses.FirstOrDefault(a => a.Id == addressId && a.IsActive);
        if (target == null)
        {
            throw new InvalidOperationException($"Endereço com identificador '{addressId}' não encontrado ou inativo.");
        }

        foreach (var addr in _addresses.Where(a => a.IsActive))
        {
            addr.SetDefault(addr.Id == addressId);
        }

        TouchUpdated();
    }

    public void RemoveAddress(Guid addressId)
    {
        var target = _addresses.FirstOrDefault(a => a.Id == addressId && a.IsActive);
        if (target == null)
        {
            throw new InvalidOperationException($"Endereço com identificador '{addressId}' não encontrado ou já inativo.");
        }

        var wasDefault = target.IsDefault;
        target.Deactivate();

        if (wasDefault)
        {
            var nextDefault = _addresses.FirstOrDefault(a => a.IsActive);
            if (nextDefault != null)
            {
                nextDefault.SetDefault(true);
            }
        }

        TouchUpdated();
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("O nome do cliente é obrigatório.", nameof(name));
        }

        var trimmed = name.Trim();
        if (trimmed.Length < 2 || trimmed.Length > 150)
        {
            throw new ArgumentOutOfRangeException(nameof(name), "O nome do cliente deve conter entre 2 e 150 caracteres.");
        }

        Name = trimmed;
    }

    private void SetPhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            Phone = null;
            return;
        }

        var trimmed = phone.Trim();
        if (trimmed.Length > 20)
        {
            throw new ArgumentOutOfRangeException(nameof(phone), "O telefone não pode exceder 20 caracteres.");
        }

        Phone = trimmed;
    }

    private void SetEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            Email = null;
            return;
        }

        var trimmed = email.Trim();
        if (trimmed.Length > 150)
        {
            throw new ArgumentOutOfRangeException(nameof(email), "O e-mail não pode exceder 150 caracteres.");
        }

        Email = trimmed;
    }

    private void SetDocument(string? document)
    {
        if (string.IsNullOrWhiteSpace(document))
        {
            Document = null;
            return;
        }

        var trimmed = document.Trim();
        if (trimmed.Length > 20)
        {
            throw new ArgumentOutOfRangeException(nameof(document), "O documento não pode exceder 20 caracteres.");
        }

        Document = trimmed;
    }
}
