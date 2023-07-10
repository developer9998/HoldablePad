using UnityEngine;

namespace HoldablePad.Scripts
{
    public class Gun
    {
        public Transform ProjectileObject;
        public AudioSource ProjectileSource;

        public bool LoopAudio, VibrationModule;
        public float ProjectCooldown, ProjectSpeed;
        public float VibrationAmp, VibrationDur;
    }
}
