using System.Net.Mail;
using Partners.Application.Exceptions;
using Partners.Domain.Enums;

namespace Partners.Application.Validators;

internal static class PartnerInputValidator
{
    public static string ValidateName(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ApplicationValidationException(
                "O nome é obrigatório.");
        }

        var name = value.Trim();

        if (name.Length is < 2 or > 100)
        {
            throw new ApplicationValidationException(
                "O nome deve ter entre 2 e 100 caracteres.");
        }

        return name;
    }

    public static string ValidateTaxNumber(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ApplicationValidationException(
                "O NIF é obrigatório.");
        }

        var normalized = NormalizeTaxNumber(value);

        if (normalized.Length != 9 ||
            normalized.Any(c => c < '0' || c > '9'))
        {
            throw new ApplicationValidationException(
                "O NIF deve conter exatamente nove dígitos ASCII.");
        }

        return normalized;
    }

    public static string NormalizeTaxNumber(string value)
    {
        return new string(
            value
                .Where(c =>
                    !char.IsWhiteSpace(c) &&
                    c != '.' &&
                    c != '-')
                .ToArray());
    }

    public static PartnerType ValidateType(string? value)
    {
        if (value is not ("CUSTOMER" or "SUPPLIER" or "BOTH"))
        {
            throw new ApplicationValidationException(
                "O Type deve ser CUSTOMER, SUPPLIER ou BOTH.");
        }

        return Enum.Parse<PartnerType>(value);
    }

    public static string? ValidateEmail(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var email = value.Trim();

        try
        {
            var address = new MailAddress(email);

            if (!string.Equals(
                    address.Address,
                    email,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new ApplicationValidationException(
                    "O formato do email é inválido.");
            }
        }
        catch (FormatException)
        {
            throw new ApplicationValidationException(
                "O formato do email é inválido.");
        }

        if (email.Length > 254)
        {
            throw new ApplicationValidationException(
                "O email não pode exceder 254 caracteres.");
        }

        return email;
    }

    public static decimal ValidateCreditLimit(decimal value)
    {
        if (value < 0 || value > 1_000_000)
        {
            throw new ApplicationValidationException(
                "O CreditLimit deve estar entre 0 e 1 000 000.");
        }

        if (decimal.Round(value, 2) != value)
        {
            throw new ApplicationValidationException(
                "O CreditLimit não pode ter mais de duas casas decimais.");
        }

        return value;
    }

    public static byte[] ValidateVersion(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ApplicationValidationException(
                "A Version é obrigatória.");
        }

        try
        {
            var version = Convert.FromBase64String(value);

            if (version.Length != 8 ||
                !string.Equals(
                    Convert.ToBase64String(version),
                    value,
                    StringComparison.Ordinal))
            {
                throw new ApplicationValidationException(
                    "A Version é inválida.");
            }

            return version;
        }
        catch (FormatException)
        {
            throw new ApplicationValidationException(
                "A Version deve ser uma string Base64 válida.");
        }
    }
}