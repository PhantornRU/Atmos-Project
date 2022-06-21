using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Integrity : MonoBehaviour
{
    [Tooltip("This object's initial \"HP\"")]
    public float initialIntegrity = 100f;
    public float integrity { get; private set; } = 100f;
    //private bool destroyed = false;
    //private DamageType lastDamageType;

    //[PrefabModeOnly]
    [Tooltip("Звук проигрываемый когда наносится урон")]
    public AudioSource soundOnHit;

    /// <summary>
    /// Наносим урон данному объекту
    /// </summary>
    /// <param name="damage"></param>
    public void ApplyDamage(float damage)//, bool explodeOnDestroy = false)
	{
		if (damage > 0)
		{
			integrity -= damage;

			//CheckDestruction(explodeOnDestroy);

			//Logger.LogTraceFormat("{0} took {1} {2} damage from {3} attack (resistance {4}) (integrity now {5})", Category.Damage, name, damage, damageType, attackType, Armor.GetRating(attackType), integrity);
		}
		//else
  //      {
		//	DestroyOrKill();
  //      }
	}

  //  public void DestroyOrKill()
  //  {
		//Destroy(this);
  //      //TryGetComponent
  //  }

    /// <summary>
    /// Directly restore integrity to this object. Final integrity will not exceed the initial integrity.
    /// </summary>
    public void RestoreIntegrity(float amountToRestore)
	{
		integrity += amountToRestore;
		if (integrity > initialIntegrity)
		{
			integrity = initialIntegrity;
		}
	}

	//private void CheckDestruction(bool explodeOnDestroy = false)
	//{
	//	if (!destroyed && integrity <= 0)
	//	{
	//		Profiler.BeginSample("IntegrityOnWillDestroy");
	//		var destructInfo = new DestructionInfo(lastDamageType, this);
	//		OnWillDestroyServer.Invoke(destructInfo);
	//		Profiler.EndSample();

	//		if (onFire)
	//		{
	//			//ensure we stop burning
	//			SyncOnFire(onFire, false);
	//		}

	//		if (explodeOnDestroy)
	//		{
	//			Explosion.StartExplosion(registerTile.WorldPositionServer, ExplosionsDamage);
	//		}

	//		if (destructInfo.DamageType == DamageType.Burn)
	//		{
	//			if (OnBurnUpServer != null)
	//			{
	//				OnBurnUpServer(destructInfo);
	//			}
	//			else
	//			{
	//				DefaultBurnUp(destructInfo);
	//			}
	//		}
	//		else
	//		{
	//			DefaultDestroy(destructInfo);
	//		}

	//		destroyed = true;
	//	}
	//}

}
