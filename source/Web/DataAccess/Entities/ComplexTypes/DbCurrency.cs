using Karambolo.Common.Finances;

namespace AspNetSkeleton.DataAccess.Entities.ComplexTypes
{
    public class DbCurrency
    {
        public DbCurrency() { }

        public DbCurrency(Currency currency)
        {
            _currency = currency;
        }

        Currency _currency;

        public string Value
        {
            get => _currency.Code;
            set => _currency = Currency.FromCode(value);
        }

        public Currency AsCurrency()
        {
            return _currency;
        }
    }
}
