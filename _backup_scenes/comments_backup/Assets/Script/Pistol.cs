using UnityEngine;

/// <summary>
/// 手枪 — 单发远程武器，在 WeaponBase 基础上叠加枪口闪光
/// </summary>
public class Pistol : WeaponBase
{
    [Header("手枪特效")]
    [SerializeField] private GameObject muzzleFlash;

    protected override void Fire()
    {
        base.Fire();

        if (muzzleFlash != null)
        {
            GameObject flash = Instantiate(muzzleFlash, transform.position, transform.rotation);
            Destroy(flash, 0.1f);
        }
    }
}
