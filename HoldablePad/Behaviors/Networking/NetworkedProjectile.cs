using GorillaExtensions;
using UnityEngine;

namespace HoldablePad.Behaviors.Networking
{
    public class NetworkedProjectile : MonoBehaviour
    {
        public Vector3 targetVelocity;
        public NetworkedGun ReferenceHoldable;
        public Rigidbody ProjectileRigidbody;

        public void Start()
        {
            gameObject.layer = LayerMask.NameToLayer("GorillaThrowable");
            gameObject.GetOrAddComponent<SphereCollider>().radius = 0.015f;
            var RefGun = ReferenceHoldable.ReferenceGun;
            Destroy(gameObject, RefGun.ProjectSpeed * 0.3f);

            ProjectileRigidbody = gameObject.AddComponent<Rigidbody>();
            ProjectileRigidbody.mass = 10;
            ProjectileRigidbody.constraints = RigidbodyConstraints.FreezeRotation;
            ProjectileRigidbody.velocity = targetVelocity;
        }

        public void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.TryGetComponent<HitTargetWithScoreCounter>(out var hitTargetWithScoreCounter))
            {
                // This is done client sided, so it shouldn't do much
                // EDIT: Other players with the mod with the gun will also see these changes, but no one else. I also found out it can be used to pop balloons and splash water, 7:42 AM 7/2/2023
                // https://cdn.discordapp.com/attachments/1053828669015085128/1124978242206584934/2023-07-02_04-20-49.mp4
                hitTargetWithScoreCounter.TargetHit();
            }
            Destroy(gameObject);
        }

        public int SecondsToMilliseconds(float seconds)
            => Mathf.FloorToInt(seconds * 1000 + 0.5f);
    }
}
