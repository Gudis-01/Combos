using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CubeCombo : MonoBehaviour
{
    [System.Serializable]
    public class Combo
    {
        public string name;
        public KeyCode[] sequence;
        public Vector3 velocityChange;        // salto (mundo)
        public Vector3 angularVelocityChange; // giro en el aire
    }

    [Tooltip("Tiempo máximo (s) entre una tecla y la siguiente")]
    public float comboWindow = 0.5f;
    public Combo[] combos;

    private Rigidbody _rb;
    private readonly List<KeyCode> _buffer = new List<KeyCode>();
    private readonly HashSet<KeyCode> _comboKeys = new HashSet<KeyCode>();
    private float _lastKeyTime;
    private int _contacts;

    private bool Grounded => _contacts > 0;

    // Se ejecuta al agregar el componente o con "Reset" en el menú de tres puntos
    void Reset()
    {
        combos = new[]
        {
            new Combo
            {
                name = "Salto izquierda",
                sequence = new[] { KeyCode.UpArrow, KeyCode.UpArrow, KeyCode.UpArrow },
                velocityChange = new Vector3(-2.5f, 5f, 0f),
                angularVelocityChange = new Vector3(0f, 0f, 3.1f)
            },
            new Combo
            {
                name = "Salto derecha",
                sequence = new[] { KeyCode.UpArrow, KeyCode.UpArrow, KeyCode.DownArrow,
                                   KeyCode.DownArrow, KeyCode.Q, KeyCode.A },
                velocityChange = new Vector3(2.5f, 5f, 0f),
                angularVelocityChange = new Vector3(0f, 0f, -3.1f)
            },
            new Combo
            {
                name = "Salto recto",
                sequence = new[] { KeyCode.UpArrow, KeyCode.UpArrow, KeyCode.UpArrow,
                                   KeyCode.DownArrow, KeyCode.Q, KeyCode.A },
                velocityChange = new Vector3(0f, 6f, 0f),
                angularVelocityChange = Vector3.zero
            }
        };
    }

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        foreach (Combo c in combos)
            foreach (KeyCode k in c.sequence)
                _comboKeys.Add(k);
    }

    void Update()
    {
        foreach (KeyCode key in _comboKeys)
        {
            if (Input.GetKeyDown(key))
                RegisterKey(key);
        }

        // Si pasó el tiempo, dispara el combo que coincida exacto (si lo hay) y reinicia
        if (_buffer.Count > 0 && Time.time - _lastKeyTime > comboWindow)
        {
            Combo exact = FindExact();
            if (exact != null) Fire(exact);
            _buffer.Clear();
        }
    }

    void RegisterKey(KeyCode key)
    {
        _buffer.Add(key);
        _lastKeyTime = Time.time;

        // Si ya no es el inicio de ningún combo, reinicia (y prueba con esta tecla sola)
        if (!AnyComboStartsWith(_buffer))
        {
            _buffer.Clear();
            _buffer.Add(key);
            if (!AnyComboStartsWith(_buffer))
            {
                _buffer.Clear();
                return;
            }
        }

        // Dispara de inmediato solo si ningún combo más largo puede continuar
        Combo exact = FindExact();
        if (exact != null && !AnyLongerComboStartsWith(_buffer))
        {
            Fire(exact);
            _buffer.Clear();
        }
    }

    void Fire(Combo combo)
    {
        if (!Grounded) return;
        _rb.AddForce(combo.velocityChange, ForceMode.VelocityChange);
        _rb.AddTorque(combo.angularVelocityChange, ForceMode.VelocityChange);
    }

    // ---------- Búsqueda de combos ----------
    bool StartsWith(KeyCode[] seq, List<KeyCode> buf)
    {
        if (seq.Length < buf.Count) return false;
        for (int i = 0; i < buf.Count; i++)
            if (seq[i] != buf[i]) return false;
        return true;
    }

    bool AnyComboStartsWith(List<KeyCode> buf)
    {
        foreach (Combo c in combos)
            if (StartsWith(c.sequence, buf)) return true;
        return false;
    }

    bool AnyLongerComboStartsWith(List<KeyCode> buf)
    {
        foreach (Combo c in combos)
            if (c.sequence.Length > buf.Count && StartsWith(c.sequence, buf)) return true;
        return false;
    }

    Combo FindExact()
    {
        foreach (Combo c in combos)
            if (c.sequence.Length == _buffer.Count && StartsWith(c.sequence, _buffer)) return c;
        return null;
    }

    // ---------- Detección de suelo ----------
    void OnCollisionEnter(Collision _) => _contacts++;
    void OnCollisionExit(Collision _) => _contacts = Mathf.Max(0, _contacts - 1);
}