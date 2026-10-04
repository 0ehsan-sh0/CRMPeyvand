namespace BE
{
    /// <summary>
    /// Persian captions for PaymentInstrument.
    ///
    /// The grid row and the printed receipt must call one instrument by one word,
    /// and BE is the only assembly both the DAL (which builds grid rows) and the
    /// UI (which builds the receipt) can see. A switch rather than a dictionary
    /// because the enum is closed, so a missing case shows up in review instead of
    /// rendering blank.
    /// </summary>
    public static class PaymentInstrumentTitles
    {
        public static string Of(PaymentInstrument instrument)
        {
            switch (instrument)
            {
                case PaymentInstrument.Cash: return "نقد";
                case PaymentInstrument.Card: return "کارت";
                case PaymentInstrument.Transfer: return "حواله";
                case PaymentInstrument.Cheque: return "چک";
                case PaymentInstrument.Other: return "سایر";
                default: return "نامشخص";
            }
        }
    }
}