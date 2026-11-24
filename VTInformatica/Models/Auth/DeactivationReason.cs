namespace VTInformatica.Models.Auth
{
    /// <summary>Chi ha disattivato l'account. Parte da 1: lo 0 di default non ha significato.</summary>
    public enum DeactivationReason
    {
        /// <summary>Il cliente ha cancellato il proprio account: può riattivarlo con la password.</summary>
        User = 1,

        /// <summary>Sospensione da parte di un Admin: solo un Admin può revocarla.</summary>
        Admin = 2
    }
}
