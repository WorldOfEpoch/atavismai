using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Atavism
{
    public class PROPS
    {
        public static readonly string CHARACTER_NAME = "characterName";
        public static readonly string CHARACTER_CLASS = "aspect";
        public static readonly string CHARACTER_CLASS_ID = "aspectId";
        public static readonly string CHARACTER_RACE = "race";
        public static readonly string CHARACTER_RACE_ID = "raceId";
        public static readonly string CHARACTER_GENDER = "gender";
        public static readonly string CHARACTER_GENDER_ID = "genderId";
        public static string CHARACTER_CUSTOM(string property) => "custom:" + property;

        public static readonly string PORTRAIT = "portrait";
        public static readonly string TITLE = "displayName";
        public static readonly string SUB_TITLE = "subTitle";
        public static readonly string SPECIES = "species";
        public static readonly string LEVEL = "level";
        public static readonly string HEALTH = "health";
        public static readonly string HEALTH_MAX = "health-max";
        public static readonly string MANA = "mana";
        public static readonly string MANA_MAX = "mana-max";
        public static readonly string WEIGHT = "weight";
        public static readonly string WEIGHT_MAX = "weight-max";
        public static readonly string STAMINA = "stamina";
        public static readonly string STAMINA_MAX = "stamina-max";
        public static readonly string PET = "pet";
        public static readonly string PET_LEVEL = "petLevel";
        public static readonly string PET_OWNER = "petOwner";

        public static readonly string EFFECTS = "effects";
        public static readonly string REACTION = "reaction";
        public static readonly string AGGRESSIVE = "aggressive";
    }
}