namespace NonStandard.GameUi.Inventory.Items {
	public class Weapon : Base {
		public float damage = 3;
		public float delay = 1;
		public enum Type { None, Natural, Bludgeoning, Slashing, Piercing, Projectile }
		public Type type;
	}
}
