using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleCollisionHandler : MonoBehaviour
{
    public new ParticleSystem particleSystem;

    void Start()
    {
        // 파티클 시스템 컴포넌트 가져오기
        particleSystem = GetComponent<ParticleSystem>();
    }

    void OnParticleCollision(GameObject other)
    {
        // 충돌한 오브젝트가 파티클 시스템과 같을 경우
        if (other == gameObject)
        {
            ParticleSystem.Particle[] particles = new ParticleSystem.Particle[particleSystem.main.maxParticles];
            int numParticles = particleSystem.GetParticles(particles);

            for (int i = 0; i < numParticles; i++)
            {
                // 충돌한 파티클을 비활성화하여 사라지도록 함
                particles[i].startLifetime = 0f;
            }

            particleSystem.SetParticles(particles, numParticles);
        }
    }
}
