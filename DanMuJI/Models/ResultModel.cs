using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DanMuJI.Models
{
    public class ResultModel
    {
        public int code { get; set; }

        public string msg { get; set; }

        public string data { get; set; }
    }

    public class BiliBiliReturnModel
    {
        public string code { get; set; }

        public BiliBiliModelInfo data { get; set; }

        public string message { get; set; }

        public string msg { get; set; }
    }

    public class BiliBiliModelInfo
    {
        public int mode { get; set; }

        public int show_player_type { get; set; }

        public BiliBiliExtra extra { get; set; }
    }

    public class BiliBiliExtra
    {
        public string send_from_me { get; set; }

        public int mode { get; set; }

        public int color { get; set; }

        public int dm_type { get; set; }

        public int font_size { get; set; }

        public int player_mode { get; set; }

        public int show_player_type { get; set; }

        public string content { get; set; }

        public string user_hash { get; set; }

        public string emoticon_unique { get; set; }

        public int bulge_display { get; set; }

        public int recommend_score { get; set; }

        public string main_state_dm_color { get; set; }

        public string objective_state_dm_color { get; set; }

        public int direction { get; set; }

        public int pk_direction { get; set; }

        public int quartet_direction { get; set; }

        public int anniversary_crowd { get; set; }

        public string yeah_space_type { get; set; }

        public string yeah_space_url { get; set; }

        public string jump_to_url { get; set; }

        public string space_type { get; set; }

        public string space_url { get; set; }
    }
}