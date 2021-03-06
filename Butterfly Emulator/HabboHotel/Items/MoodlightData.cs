﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using ButterStorm;

namespace Butterfly.HabboHotel.Items
{
    class MoodlightData
    {
        internal Boolean Enabled;
        internal int CurrentPreset;
        internal List<MoodlightPreset> Presets;

        internal uint ItemId;

        internal MoodlightData(uint ItemId)
        {
            this.ItemId = ItemId;

            DataRow Row;

            using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("SELECT enabled,current_preset,preset_one,preset_two,preset_three FROM items_moodlight WHERE item_id = " + ItemId);
                Row = dbClient.getRow();
            }

            if (Row == null)
            {
                throw new NullReferenceException("No moodlightdata found in the database");
            }

            Enabled = OtanixEnvironment.EnumToBool(Row["enabled"].ToString());
            CurrentPreset = (int)Row["current_preset"];
            Presets = new List<MoodlightPreset>
            {
                GeneratePreset((string) Row["preset_one"]),
                GeneratePreset((string) Row["preset_two"]),
                GeneratePreset((string) Row["preset_three"])
            };
        }

        internal void Enable()
        {
            Enabled = true;

            using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.runFastQuery("UPDATE items_moodlight SET enabled = 1 WHERE item_id = " + ItemId);
            }
        }

        internal void Disable()
        {
            Enabled = false;

            using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.runFastQuery("UPDATE items_moodlight SET enabled = 0 WHERE item_id = " + ItemId);
            }
        }

        internal void UpdatePreset(int Preset, string Color, int Intensity, bool BgOnly)
        {
            if (!IsValidColor(Color) || !IsValidIntensity(Intensity))
            {
                return;
            }

            GetPreset(Preset).ColorCode = Color;
            GetPreset(Preset).ColorIntensity = Intensity;
            GetPreset(Preset).BackgroundOnly = BgOnly;
        }

        internal static MoodlightPreset GeneratePreset(string Data)
        {
            var Bits = Data.Split(',');

            if (!IsValidColor(Bits[0]))
            {
                Bits[0] = "#000000";
            }

            return new MoodlightPreset(Bits[0], int.Parse(Bits[1]), OtanixEnvironment.EnumToBool(Bits[2]));
        }

        internal MoodlightPreset GetPreset(int i)
        {
            i--;

            if (Presets[i] != null)
            {
                return Presets[i];
            }

            return new MoodlightPreset("#000000", 255, false);
        }

        internal static bool IsValidColor(string ColorCode)
        {
            switch (ColorCode)
            {
                case "#000000":
                case "#0053F7":
                case "#EA4532":
                case "#82F349":
                case "#74F5F5":
                case "#E759DE":
                case "#F2F851":

                    return true;

                default:

                    return false;
            }
        }

        internal static bool IsValidIntensity(int Intensity)
        {
            if (Intensity < 0 || Intensity > 255)
            {
                return false;
            }

            return true;
        }

        internal string GenerateExtraData()
        {
            var Preset = GetPreset(CurrentPreset);
            var SB = new StringBuilder();

            if (Enabled)
            {
                SB.Append(2);
            }
            else
            {
                SB.Append(1);
            }

            SB.Append(",");
            SB.Append(CurrentPreset);
            SB.Append(",");

            if (Preset.BackgroundOnly)
            {
                SB.Append(2);
            }
            else
            {
                SB.Append(1);
            }

            SB.Append(",");
            SB.Append(Preset.ColorCode);
            SB.Append(",");
            SB.Append(Preset.ColorIntensity);
            return SB.ToString();
        }
    }

    class MoodlightPreset
    {
        internal string ColorCode;
        internal int ColorIntensity;
        internal bool BackgroundOnly;

        internal MoodlightPreset(string ColorCode, int ColorIntensity, bool BackgroundOnly)
        {
            this.ColorCode = ColorCode;
            this.ColorIntensity = ColorIntensity;
            this.BackgroundOnly = BackgroundOnly;
        }
    }
}
