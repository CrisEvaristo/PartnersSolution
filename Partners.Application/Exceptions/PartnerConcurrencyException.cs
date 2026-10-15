namespace Partners.Application.Exceptions;

public sealed class PartnerConcurrencyException : Exception
{
    public PartnerConcurrencyException()
        : base("A entidade foi alterada por outro pedido. Recarregue os dados e tente novamente.")
    {
    }
}