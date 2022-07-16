using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using System.Collections;

namespace HoldablePad
{
    public class Gun : MonoBehaviour
    {
        public bool isLeftHand; // is the holdable held in the left hand?
        public GameObject BulletObject; // the gun's bullet gameobject
        public GameObject SoundObject; // a gameobject with an AudioSource
        public float Cooldown; // the cooldown for shooting
        public float speed; // the speed for shooting
        public bool holdSound; // should the audio be looped?

        public bool triggerDown; // is the isLeftHand trigger down?

        public float touchTime; // cooldown for shooting
        public float touchTime2; // cooldown for sound looping

        public bool VIBRATIONS = false; // does the gun have vibrations?
        public float VIB_STR; // the strength or amplitude of the vibration
        public float VIB_DUR; // the duration or time of the vibration

        void Awake()
        {
            BulletObject = gameObject.transform.Find("UsedBulletGameObject").gameObject;
            SoundObject = gameObject.transform.Find("UsedBulletSoundEffect").gameObject;

            string handheldData = BulletObject.GetComponent<Text>().text;
            string[] handheldInfo = handheldData.Split('$');

            speed = float.Parse(handheldInfo[0]);
            Cooldown = float.Parse(handheldInfo[1]);
            
            if (handheldInfo[2] == "True")
                holdSound = true;
            else
                holdSound = false;

            if (holdSound)
                SoundObject.GetComponent<AudioSource>().loop = true;

            for (int i = 0; i < handheldInfo.Length; i++)
            {
                if (i == 3)
                {
                    if (handheldInfo[3] == "True")
                    {
                        VIBRATIONS = true;
                        VIB_STR = float.Parse(handheldInfo[4]);
                        VIB_DUR = float.Parse(handheldInfo[5]);
                    }
                }
            }

            BulletObject.SetActive(false);
            gameObject.SetActive(false);
        }

        void FixedUpdate()
        {
            XRNode node = isLeftHand ? XRNode.LeftHand : XRNode.RightHand;
            InputDevices.GetDeviceAtXRNode(node).TryGetFeatureValue(CommonUsages.triggerButton, out triggerDown);

            if (triggerDown)
            {
                if (!(touchTime + Cooldown < Time.time))
                {
                    return;
                }

                touchTime = Time.time;

                Shoot();
            }

            if (holdSound && triggerDown && VIBRATIONS)
            {
                if (!(touchTime2 + VIB_DUR < Time.time))
                {
                    return;
                }

                touchTime2 = Time.time;
                GorillaTagger.Instance.StartVibration(isLeftHand, VIB_STR, VIB_DUR);
            }

            if (triggerDown)
            {
                if (SoundObject.GetComponent<AudioSource>().isPlaying == false && holdSound == true)
                {
                    SoundObject.GetComponent<AudioSource>().Play();
                }
            }
            else
            {
                if (SoundObject.GetComponent<AudioSource>().isPlaying == true && holdSound == true)
                {
                    SoundObject.GetComponent<AudioSource>().Stop();
                }
            }
        }

        void Shoot()
        {
            if (!holdSound)
            {
                SoundObject.GetComponent<AudioSource>().Play();
                if (VIBRATIONS)
                {
                    GorillaTagger.Instance.StartVibration(isLeftHand, VIB_STR, VIB_DUR);
                }
            }

            BulletObject.SetActive(true);
            GameObject clonedBullet = Instantiate(BulletObject);
            clonedBullet.gameObject.SetActive(true);
            clonedBullet.transform.SetParent(null, false);
            clonedBullet.transform.position = BulletObject.transform.position;
            clonedBullet.transform.rotation = BulletObject.transform.rotation;
            clonedBullet.transform.localScale = BulletObject.transform.localScale * 1.5f;
            clonedBullet.AddComponent<Bullet>().speed = speed;
            BulletObject.SetActive(false);
        }
    }

    public class Bullet : MonoBehaviour
    {
        public float speed;
        Rigidbody rb;

        Vector3 lastVelocity;
        bool keepLast = false;

        void Start()
        {
            StartCoroutine(Process());
            StartCoroutine(DespawnBullet());
        }

        IEnumerator Process()
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.velocity = GorillaLocomotion.Player.Instance.GetComponent<Rigidbody>().velocity;
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            rb.mass = gameObject.transform.localScale.y * 30;
            rb.AddRelativeForce(Vector3.forward * speed, ForceMode.VelocityChange);
            rb.AddRelativeForce(Vector3.up * speed * 0.15f, ForceMode.VelocityChange);
            yield return new WaitForSeconds(speed * 0.15f);
            lastVelocity = rb.velocity;
            keepLast = true;
            yield return new WaitForSeconds(speed * 0.15f);
            keepLast = false;
            yield break;
        }

        IEnumerator DespawnBullet()
        {
            yield return new WaitForSeconds(speed * 0.35f);
            yield return new WaitForEndOfFrame();
            Destroy(gameObject);
        }

        void Update()
        {
            if (keepLast)
            {
                rb.velocity = lastVelocity;
            }
        }
    }
}
