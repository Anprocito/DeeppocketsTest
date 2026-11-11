namespace DeepPocket.Data
{
    // source = quién hizo el daño (por ejemplo un WeaponController), para acreditar kills.
    public interface IDamageable
    {
        void TakeDamage(float amount, object source = null);
    }

    // Lo implementa quien quiere enterarse de que un daño suyo mató a alguien.
    public interface IKillListener
    {
        void OnKill();
    }
}
