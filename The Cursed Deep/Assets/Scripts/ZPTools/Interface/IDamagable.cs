namespace ZPTools.Interface
{
    public interface IDamagable
    {
        void TakeDamage(float amount);
        void TakeDamage(IDamageDealer dealer);
    }
}