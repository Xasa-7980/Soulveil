using UnityEngine;

public class ElementReactionSystem : MonoBehaviour
{
    public static ElementReactionSystem Instance { get; private set; }

    private void Awake ( )
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void TryReact ( Element currentElement, Element incomingElement, DamageInfo damageInfo, EntityInfo target )
    {
        if (currentElement == null || incomingElement == null) return;
        if (currentElement == incomingElement) return;

        if (IsCombination<WaterElement, FireElement>(currentElement, incomingElement))
        {
            WaterFireReaction(damageInfo, target);
            return;
        }

        if (IsCombination<WaterElement, ElectroElement>(currentElement, incomingElement))
        {
            WaterElectroReaction(damageInfo, target);
            return;
        }

        if (IsCombination<WaterElement, GeoElement>(currentElement, incomingElement))
        {
            WaterGeoReaction(damageInfo, target);
            return;
        }

        if (IsCombination<WaterElement, LightElement>(currentElement, incomingElement))
        {
            WaterLightReaction(damageInfo, target);
            return;
        }

        if (IsCombination<WaterElement, DarkElement>(currentElement, incomingElement))
        {
            WaterDarkReaction(damageInfo, target);
            return;
        }

        if (IsCombination<FireElement, ElectroElement>(currentElement, incomingElement))
        {
            FireElectroReaction(damageInfo, target);
            return;
        }

        if (IsCombination<FireElement, GeoElement>(currentElement, incomingElement))
        {
            FireGeoReaction(damageInfo, target);
            return;
        }

        if (IsCombination<FireElement, LightElement>(currentElement, incomingElement))
        {
            FireLightReaction(damageInfo, target);
            return;
        }

        if (IsCombination<FireElement, DarkElement>(currentElement, incomingElement))
        {
            FireDarkReaction(damageInfo, target);
            return;
        }

        if (IsCombination<ElectroElement, GeoElement>(currentElement, incomingElement))
        {
            ElectroGeoReaction(damageInfo, target);
            return;
        }

        if (IsCombination<ElectroElement, LightElement>(currentElement, incomingElement))
        {
            ElectroLightReaction(damageInfo, target);
            return;
        }

        if (IsCombination<ElectroElement, DarkElement>(currentElement, incomingElement))
        {
            ElectroDarkReaction(damageInfo, target);
            return;
        }

        if (IsCombination<GeoElement, LightElement>(currentElement, incomingElement))
        {
            GeoLightReaction(damageInfo, target);
            return;
        }

        if (IsCombination<GeoElement, DarkElement>(currentElement, incomingElement))
        {
            GeoDarkReaction(damageInfo, target);
            return;
        }

        if (IsCombination<LightElement, DarkElement>(currentElement, incomingElement))
        {
            LightDarkReaction(damageInfo, target);
            return;
        }
    }

    private void WaterFireReaction ( DamageInfo damageInfo, EntityInfo target )
    {
        DebugReaction("Water + Fire", damageInfo, target);

        if (target.health != null)
        {
            // efecto sobre vida
        }

        if (target.stats != null)
        {
            // modificar stats
        }

        if (target.element != null)
        {
            // consultar/cambiar elemento
        }
    }
    private void WaterElectroReaction ( DamageInfo damageInfo, EntityInfo target )
    {
        DebugReaction("Water + Electro", damageInfo, target);
    }

    private void WaterGeoReaction ( DamageInfo damageInfo, EntityInfo target )
    {
        DebugReaction("Water + Geo", damageInfo, target);
    }

    private void WaterLightReaction ( DamageInfo damageInfo, EntityInfo target )
    {
        DebugReaction("Water + Light", damageInfo, target);
    }

    private void WaterDarkReaction ( DamageInfo damageInfo, EntityInfo target )
    {
        DebugReaction("Water + Dark", damageInfo, target);
    }

    private void FireElectroReaction ( DamageInfo damageInfo, EntityInfo target )
    {
        DebugReaction("Fire + Electro", damageInfo, target);
    }

    private void FireGeoReaction ( DamageInfo damageInfo, EntityInfo target )
    {
        DebugReaction("Fire + Geo", damageInfo, target);
    }

    private void FireLightReaction ( DamageInfo damageInfo, EntityInfo target )
    {
        DebugReaction("Fire + Light", damageInfo, target);
    }

    private void FireDarkReaction ( DamageInfo damageInfo, EntityInfo target )
    {
        DebugReaction("Fire + Dark", damageInfo, target);
    }

    private void ElectroGeoReaction ( DamageInfo damageInfo, EntityInfo target )
    {
        DebugReaction("Electro + Geo", damageInfo, target);
    }

    private void ElectroLightReaction ( DamageInfo damageInfo, EntityInfo target )
    {
        DebugReaction("Electro + Light", damageInfo, target);
    }

    private void ElectroDarkReaction ( DamageInfo damageInfo, EntityInfo target )
    {
        DebugReaction("Electro + Dark", damageInfo, target);
    }

    private void GeoLightReaction ( DamageInfo damageInfo, EntityInfo target )
    {
        DebugReaction("Geo + Light", damageInfo, target);
    }

    private void GeoDarkReaction ( DamageInfo damageInfo, EntityInfo target )
    {
        DebugReaction("Geo + Dark", damageInfo, target);
    }

    private void LightDarkReaction ( DamageInfo damageInfo, EntityInfo target )
    {
        DebugReaction("Light + Dark", damageInfo, target);
    }

    private void DebugReaction ( string reactionName, DamageInfo damageInfo, EntityInfo target )
    {
        string targetName = target.entity != null ? target.entity.name : "Unknown";
        string attackerName = damageInfo.attacker != null ? damageInfo.attacker.name : "Unknown";

        Debug.Log($"{reactionName} reaction | Target: {targetName} | Attacker: {attackerName} | Position: {damageInfo.hitPoint}");
    }

    private bool IsCombination<T1, T2> ( Element elementA, Element elementB ) where T1 : Element where T2 : Element
    {
        return (elementA is T1 && elementB is T2) || (elementA is T2 && elementB is T1);
    }
}