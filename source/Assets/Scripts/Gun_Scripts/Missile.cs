using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Xml.Schema;

public class Missile : NetworkBehaviour
{
    [Header("Missile Movement")]
    [SerializeField] float m_speed = 20f;

    [Header("Explosion Values")]
    [SerializeField] int m_explosionDamage = 20;
    [SerializeField] float m_explosionRadius = 5f;
    [SerializeField] float m_explosionForce = 20f;

    [Header("References")]
    [SerializeField] GameObject m_explosionParticle;
    [SerializeField] AudioSource m_audioPlayer;
    [SerializeField] AudioClip m_explosionSound;
    [SerializeField] GameObject m_trailEffect;

    PlayerBase m_ownerPlayer;

    private void Start()
    {
        GetComponent<Rigidbody>().velocity = transform.forward * m_speed;
    }

    [Server]
    public void SetupMissile(NetworkIdentity ownerIdentity, PlayerBase ownerPlayer)
    {
        m_ownerPlayer = ownerPlayer;
        GetComponent<CapsuleCollider>().enabled = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(CollidedWithOwnerPlayer(collision))
        {
            return;
        }

        GetComponent<CapsuleCollider>().enabled = false;

        NetworkIdentity hitObjectId = collision.gameObject.GetComponent<NetworkIdentity>();

        if (isServer)
        {
            CheckForDamageableInRange();
        }

        RpcExplodeAndDestroy();
    }

    [Server]
    void CheckForDamageableInRange()
    {
        Collider[] objectsInRange = Physics.OverlapSphere(transform.position, m_explosionRadius);

        foreach(var collider in objectsInRange)
        {
            if(collider.gameObject.CompareTag("Player"))
            {
                if (collider.gameObject.GetComponent<PlayerBase>() != m_ownerPlayer)
                { 
                    DamagePlayer(collider.gameObject);
                }

                PushbackPlayer(collider.gameObject);
            }
        }
    }

    [Server]
    void DamagePlayer(GameObject player)
    {
        player.gameObject.GetComponent<PlayerBase>().TakeDamage(m_explosionDamage, m_ownerPlayer);
    }

    [Server]
    void PushbackPlayer(GameObject player)
    {
        player.GetComponent<PlayerBase>().RpcAddVelocity((player.transform.position - transform.position).normalized * (m_explosionRadius - (player.transform.position - transform.position).magnitude) * m_explosionForce);
    }

    [ClientRpc]
    void RpcExplodeAndDestroy()
    {
        Instantiate(m_explosionParticle, transform.position, transform.rotation);

        GetComponent<CapsuleCollider>().enabled = false;
        GetComponent<MeshRenderer>().enabled = false;
        m_trailEffect.SetActive(false);

        PlayExplosionSound();

        StartCoroutine(DelayThenDestroy());
    }

    [Client]
    IEnumerator DelayThenDestroy()
    {
        yield return new WaitForSeconds(5f);

        CmdDestroy();
    }

    [Client]
    void PlayExplosionSound()
    {
        m_audioPlayer.clip = m_explosionSound;
        m_audioPlayer.pitch = Random.Range(.8f, 1.2f);
        m_audioPlayer.Play();
    }

    [Command]
    private void CmdDestroy()
    {
        Destroy(gameObject);
    }

    bool CollidedWithOwnerPlayer(Collision collider)
    {
        var collidedScript = collider.gameObject.GetComponent<PlayerBase>();

        if(collidedScript == null)
        {
            return false;
        }

        if(collidedScript.Equals(m_ownerPlayer))
        {
            return true;
        }

        return false;
    }
}
