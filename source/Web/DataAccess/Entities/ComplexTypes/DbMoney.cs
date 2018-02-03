using Karambolo.Common.Finances;

namespace AspNetSkeleton.DataAccess.Entities.ComplexTypes
{
    public class DbMoney
    {
        static long IntPow(long x, byte y)
        {
            var result = 1L;
            while (y != 0)
            {
                if ((y & 1) == 1)
                    result *= x;
                x *= x;
                y >>= 1;
            }
            return result;
        }

        static decimal Scale(decimal amount, Currency currency, bool up)
        {
            var scaleExp = currency.DefaultDecimals - DataAccessConstants.MoneyScale;
            if (scaleExp <= 0)
                return amount;

            var scale = IntPow(10, (byte)scaleExp);
            return up ? amount * scale : amount / scale;
        }

        public DbMoney() { }

        public DbMoney(Money money)
        {
            _money = money;
        }

        public DbMoney(decimal amount, Currency currency)
        {
            _money = new Money(amount, currency);
        }

        Money _money;

        public string Currency
        {
            get => _money.Currency.Code;
            set => _money = Money.ChangeCurrency(_money, value);
        }

        public decimal Amount
        {
            get => Scale(_money.Amount, _money.Currency, up: true);
            set => _money = Money.ChangeAmount(_money, Scale(value, _money.Currency, up: false));
        }

        public Money AsMoney()
        {
            return _money;
        }
    }
}
