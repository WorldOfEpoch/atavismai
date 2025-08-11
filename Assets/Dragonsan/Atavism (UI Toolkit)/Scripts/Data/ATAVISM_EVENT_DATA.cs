using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Atavism
{
    public class EVENTS
    {
        // Core
        public static readonly string LOGIN = "LOGIN_RESPONSE"; // args[0] => error types 
        public static readonly string REGISTER = "REGISTER_RESPONSE"; // args[0] => error types 
        public static readonly string SETTINGS_LOADED = "SETTINGS_LOADED";
        public static readonly string UPDATE_LANGUAGE = "UPDATE_LANGUAGE";
        
        // Messages
        public static readonly string WORLD_ERROR = "World_Error";
        public static readonly string ADMIN_MESSAGE = "ADMIN_MESSAGE";
        public static readonly string ERROR_MESSAGE = "ERROR_MESSAGE";
        public static readonly string REGION_MESSAGE = "REGION_MESSAGE";
        public static readonly string ANNOUNCEMENT_MESSAGE = "ANNOUNCEMENT_SPECIAL";
        
        // Chat
        public static readonly string CHAT_MSG_SERVER = "CHAT_MSG_SERVER";
        public static readonly string CHAT_MSG_SAY = "CHAT_MSG_SAY";
        public static readonly string CHAT_MSG_SYSTEM = "CHAT_MSG_SYSTEM";

        // Combat
        public static readonly string PLAYER_TARGET_CHANGED = "PLAYER_TARGET_CHANGED";
        public static readonly string PLAYER_TARGET_CLEARED = "TARGET_CLEAR";
        public static readonly string OBJECT_TARGET_CHANGED = "OBJECT_TARGET_CHANGED";
        public static readonly string EFFECT_UPDATE = "EFFECT_UPDATE";
        public static readonly string EFFECT_ICON_UPDATE = "EFFECT_ICON_UPDATE";
        public static readonly string COMBAT = "COMBAT_EVENT";

        // Group
        public static readonly string GROUP_UPDATE = "GROUP_UPDATE";
        public static readonly string GROUP_INVITE_REQUEST = "GROUP_INVITE_REQUEST"; // args[3] = oid, leader name, timeout
        public static readonly string GROUP_DICE = "GROUP_DICE";
        public static readonly string GROUP_UPDATE_SETTINGS = "GROUP_UPDATE_SETTINGS";

        // Inventory
        public static readonly string INVENTORY = "INVENTORY_EVENT";




        //Pending
        public static readonly string LOAD_PREFAB = "LOAD_PREFAB";
        public static readonly string ABILITY_UPDATE = "ABILITY_UPDATE";
        public static readonly string INVENTORY_UPDATE = "INVENTORY_UPDATE";
        public static readonly string MOUSE_SENSITIVE = "MOUSE_SENSITIVE";
        public static readonly string ITEM_ICON_UPDATE = "ITEM_ICON_UPDATE";
        public static readonly string ITEM_RELOAD = "ITEM_RELOAD";
        public static readonly string CURRENCY_ICON_UPDATE = "CURRENCY_ICON_UPDATE";
        public static readonly string SKILL_ICON_UPDATE = "SKILL_ICON_UPDATE";
        public static readonly string CASTING_CANCELLED = "CASTING_CANCELLED";
        public static readonly string LOADING_SCENE_START = "LOADING_SCENE_START";
        public static readonly string LOADING_SCENE_END = "LOADING_SCENE_END";
        public static readonly string PLAYER_TELEPORTED = "PLAYER_TELEPORTED";
        public static readonly string LOGIN_RESPONSE = "LOGIN_RESPONSE";
        public static readonly string REGISTER_RESPONSE = "REGISTER_RESPONSE";
        public static readonly string WORLD_TIME_UPDATE = "WORLD_TIME_UPDATE";
        public static readonly string ACHIEVEMENT_UPDATE = "ACHIEVEMENT_UPDATE";
        public static readonly string ACHIEV_UPDATE = "ACHIEV_UPDATE";
        public static readonly string ACTION_UPDATE = "ACTION_UPDATE";
        public static readonly string COOLDOWN_UPDATE = "COOLDOWN_UPDATE";
        public static readonly string SETTINGS = "SETTINGS";
        public static readonly string ANNOUNCEMENT_SPECIAL = "ANNOUNCEMENT_SPECIAL";
        public static readonly string ANNOUNCEMENT = "ANNOUNCEMENT";
        public static readonly string INVENTORY_EVENT = "INVENTORY_EVENT";
        public static readonly string ARENA_LIST_UPDATE = "ARENA_LIST_UPDATE";
        public static readonly string ARENA_SCORE_SETUP = "ARENA_SCORE_SETUP";
        public static readonly string ARENA_SCORE_UPDATE = "ARENA_SCORE_UPDATE";
        public static readonly string ATOGGLE_UPDATE = "ATOGGLE_UPDATE";
        public static readonly string AUCTION_OPEN = "AUCTION_OPEN";
        public static readonly string AUCTION_LIST_UPDATE = "AUCTION_LIST_UPDATE";
        public static readonly string AUCTION_LIST_FOR_GROUP_UPDATE = "AUCTION_LIST_FOR_GROUP_UPDATE";
        public static readonly string AUCTION_OWN_LIST_UPDATE = "AUCTION_OWN_LIST_UPDATE";
        public static readonly string BANK_UPDATE = "BANK_UPDATE";
        public static readonly string CLOSE_STORAGE_WINDOW = "CLOSE_STORAGE_WINDOW";
        public static readonly string CLAIM_OBJECT_SELECTED = "CLAIM_OBJECT_SELECTED";
        public static readonly string CLAIM_OBJECT_UPDATED = "CLAIM_OBJECT_UPDATED";
        public static readonly string CASTING_STARTED = "CASTING_STARTED";
        public static readonly string EQUIPPED_UPDATE = "EQUIPPED_UPDATE";
        public static readonly string COMBAT_EVENT = "COMBAT_EVENT";
        public static readonly string KEY_UPDATE_VIEW = "KEY_UPDATE_VIEW";
        public static readonly string CRAFTING_GRID_UPDATE = "CRAFTING_GRID_UPDATE";
        public static readonly string CRAFTING_RECIPE_UPDATE = "CRAFTING_RECIPE_UPDATE";
        public static readonly string CRAFTING_START = "CRAFTING_START";
        public static readonly string CLOSE_CRAFTING_STATION = "CLOSE_CRAFTING_STATION";
        public static readonly string SKILL_UPDATE = "SKILL_UPDATE";
        public static readonly string CURRENCY_UPDATE = "CURRENCY_UPDATE";
        public static readonly string NPC_INTERACTIONS_UPDATE = "NPC_INTERACTIONS_UPDATE";
        public static readonly string DIALOGUE_UPDATE = "DIALOGUE_UPDATE";
        public static readonly string QUEST_OFFERED_UPDATE = "QUEST_OFFERED_UPDATE";
        public static readonly string QUEST_PROGRESS_UPDATE = "QUEST_PROGRESS_UPDATE";
        public static readonly string CLOSE_NPC_DIALOGUE = "CLOSE_NPC_DIALOGUE";
        public static readonly string MERCHANT_UI_OPENED = "MERCHANT_UI_OPENED";
        public static readonly string GEAR_MODIFICATION_OPEN = "GEAR_MODIFICATION_OPEN";
        public static readonly string GLOABL_EVENTS_ICON = "GLOABL_EVENTS_ICON";
        public static readonly string GLOABL_EVENTS_UPDATE = "GLOABL_EVENTS_UPDATE";
        public static readonly string GUILD_UPDATE = "GUILD_UPDATE";
        public static readonly string GUILD_RES_UPDATE = "GUILD_RES_UPDATE";
        public static readonly string LOADING_PREFAB_UPDATE = "LOADING_PREFAB_UPDATE";
        public static readonly string LOADING_PREFAB_SHOW = "LOADING_PREFAB_SHOW";
        public static readonly string LOADING_PREFAB_HIDE = "LOADING_PREFAB_HIDE";
        public static readonly string LOOT_UPDATE = "LOOT_UPDATE";
        public static readonly string CLOSE_LOOT_WINDOW = "CLOSE_LOOT_WINDOW";
        public static readonly string MAILBOX_OPEN = "MAILBOX_OPEN";
        public static readonly string CLOSE_MAIL_WINDOW = "CLOSE_MAIL_WINDOW";
        public static readonly string MAIL_SENT = "MAIL_SENT";
        public static readonly string SHOW_MAIL = "SHOW_MAIL";
        public static readonly string MAIL_UPDATE = "MAIL_UPDATE";
        public static readonly string MAIL_SELECTED = "MAIL_SELECTED";
        public static readonly string CLOSE_READ_MAIL_WINDOW = "CLOSE_READ_MAIL_WINDOW";
        public static readonly string MERCHANT_UPDATE = "MERCHANT_UPDATE";
        public static readonly string NO_NEW_MAIL = "NO_NEW_MAIL";
        public static readonly string NEW_MAIL = "NEW_MAIL";
        public static readonly string MAILBOX_OPENED = "MAILBOX_OPENED";
        public static readonly string CLAIM_LIST_UPDATE = "CLAIM_LIST_UPDATE";
        public static readonly string SHOP_LIST_UPDATE = "SHOP_LIST_UPDATE";
        public static readonly string QUEST_LOG_UPDATE = "QUEST_LOG_UPDATE";
        public static readonly string QUEST_ITEM_UPDATE = "QUEST_ITEM_UPDATE";
        public static readonly string QUEST_LOG_LIST_UPDATE = "QUEST_LOG_LIST_UPDATE";
        public static readonly string REPAIR_COMPLETE = "REPAIR_COMPLETE";
        public static readonly string REPAIR_START = "REPAIR_START";
        public static readonly string RESOURCE_LOOT_UPDATE = "RESOURCE_LOOT_UPDATE";
        public static readonly string CLOSE_RESOURCE_LOOT_WINDOW = "CLOSE_RESOURCE_LOOT_WINDOW";
        public static readonly string SHOP_UPDATE = "SHOP_UPDATE";
        public static readonly string CLOSE_SHOP = "CLOSE_SHOP";
        public static readonly string SOCIAL_UPDATE = "SOCIAL_UPDATE";
        public static readonly string TimerStart = "TimerStart";
        public static readonly string TimerStop = "TimerStop";
        public static readonly string TRADE_START = "TRADE_START";
        public static readonly string TRADE_OFFER_UPDATE = "TRADE_OFFER_UPDATE";
        public static readonly string TRADE_COMPLETE = "TRADE_COMPLETE";
        public static readonly string VIP_UPDATE = "VIP_UPDATE";
        public static readonly string VIPS_UPDATE = "VIPS_UPDATE";
        public static readonly string WINDOWS_RESET = "WINDOWS_RESET";
        public static readonly string CLAIM_CHANGED = "CLAIM_CHANGED";
        public static readonly string CLAIM_UPGRADE_SHOW = "CLAIM_UPGRADE_SHOW";
        public static readonly string CLAIM_TAX_SHOW = "CLAIM_TAX_SHOW";
        public static readonly string WORLD_TIME_UPDATE_WAPI = "WORLD_TIME_UPDATE_WAPI";
        public static readonly string CLAIM_TARGET_CLEAR = "CLAIM_TARGET_CLEAR";
        public static readonly string TARGET_CLEAR = "TARGET_CLEAR";
        public static readonly string CLAIM_ADDED = "CLAIM_ADDED";
        public static readonly string CLAIMED_REMOVED = "CLAIMED_REMOVED";
        public static readonly string LOGGED_OUT = "LOGGED_OUT";
        public static readonly string PLACE_CLAIM_OBJECT = "PLACE_CLAIM_OBJECT";
        public static readonly string CLAIM_OBJECT_CLICKED = "CLAIM_OBJECT_CLICKED";
        public static readonly string START_BUILD_CLAIM_OBJECT = "START_BUILD_CLAIM_OBJECT";
        public static readonly string BUILDER_UPDATE = "BUILDER_UPDATE";
        public static readonly string BUILDER_ICON_UPDATE = "BUILDER_ICON_UPDATE";
    }
}