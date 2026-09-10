using UnityEngine;

public enum ElementReactionType
{
    None,

    Water_Fire,
    Water_Electro,
    Water_Geo,
    Water_Light,
    Water_Dark,

    Fire_Electro,
    Fire_Geo,
    Fire_Light,
    Fire_Dark,

    Electro_Geo,
    Electro_Light,
    Electro_Dark,

    Geo_Light,
    Geo_Dark,

    Light_Dark
}

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

    public ElementReactionType TryReact ( Element currentElement, Element incomingElement, DamageInfo damageInfo, EntityInfo target )
    {
        if (currentElement == null || incomingElement == null) return ElementReactionType.None;
        if (currentElement == incomingElement) return ElementReactionType.None;

        if (IsCombination<WaterElement, FireElement>(currentElement, incomingElement))
        {
            WaterFireReaction(damageInfo, target);
            return ElementReactionType.Water_Fire;
        }

        if (IsCombination<WaterElement, ElectroElement>(currentElement, incomingElement))
        {
            WaterElectroReaction(damageInfo, target);
            return ElementReactionType.Water_Electro;
        }

        if (IsCombination<WaterElement, GeoElement>(currentElement, incomingElement))
        {
            WaterGeoReaction(damageInfo, target);
            return ElementReactionType.Water_Geo;
        }

        if (IsCombination<WaterElement, LightElement>(currentElement, incomingElement))
        {
            WaterLightReaction(damageInfo, target);
            return ElementReactionType.Water_Light;
        }

        if (IsCombination<WaterElement, DarkElement>(currentElement, incomingElement))
        {
            WaterDarkReaction(damageInfo, target);
            return ElementReactionType.Water_Dark;
        }

        if (IsCombination<FireElement, ElectroElement>(currentElement, incomingElement))
        {
            FireElectroReaction(damageInfo, target);
            return ElementReactionType.Fire_Electro;
        }

        if (IsCombination<FireElement, GeoElement>(currentElement, incomingElement))
        {
            FireGeoReaction(damageInfo, target);
            return ElementReactionType.Fire_Geo;
        }

        if (IsCombination<FireElement, LightElement>(currentElement, incomingElement))
        {
            FireLightReaction(damageInfo, target);
            return ElementReactionType.Fire_Light;
        }

        if (IsCombination<FireElement, DarkElement>(currentElement, incomingElement))
        {
            FireDarkReaction(damageInfo, target);
            return ElementReactionType.Fire_Dark;
        }

        if (IsCombination<ElectroElement, GeoElement>(currentElement, incomingElement))
        {
            ElectroGeoReaction(damageInfo, target);
            return ElementReactionType.Electro_Geo;
        }

        if (IsCombination<ElectroElement, LightElement>(currentElement, incomingElement))
        {
            ElectroLightReaction(damageInfo, target);
            return ElementReactionType.Electro_Light;
        }

        if (IsCombination<ElectroElement, DarkElement>(currentElement, incomingElement))
        {
            ElectroDarkReaction(damageInfo, target);
            return ElementReactionType.Electro_Dark;
        }

        if (IsCombination<GeoElement, LightElement>(currentElement, incomingElement))
        {
            GeoLightReaction(damageInfo, target);
            return ElementReactionType.Geo_Light;
        }

        if (IsCombination<GeoElement, DarkElement>(currentElement, incomingElement))
        {
            GeoDarkReaction(damageInfo, target);
            return ElementReactionType.Geo_Dark;
        }

        if (IsCombination<LightElement, DarkElement>(currentElement, incomingElement))
        {
            LightDarkReaction(damageInfo, target);
            return ElementReactionType.Light_Dark;
        }

        return ElementReactionType.None;
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