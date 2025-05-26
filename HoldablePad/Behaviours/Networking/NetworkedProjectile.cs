using GorillaExtensions;
using UnityEngine;

namespace HoldablePad.Behaviours.Networking
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
            if (collision.gameObject.TryGetComponent(out HitTargetNetworkState networkState))
            {
                // Get the launch point and hit point
                Vector3 launchPoint = transform.position;
                Vector3 hitPoint = collision.contacts[0].point;

                networkState.TargetHit(launchPoint, hitPoint);
            }
            Destroy(gameObject);
        }


        public int SecondsToMilliseconds(float seconds)
            => Mathf.FloorToInt(seconds * 1000 + 0.5f);
    }
}
