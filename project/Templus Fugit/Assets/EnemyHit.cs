using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHit : MonoBehaviour
{
  public int damage = 1;

  private void OnTriggerEnter2D(Collider2D other)
  {
    if (other.CompareTag("Player"))
    {
      var pc = other.GetComponent<PlayerController>();
      if (pc != null) pc.TakeDamage(damage);
    }
  }
}
