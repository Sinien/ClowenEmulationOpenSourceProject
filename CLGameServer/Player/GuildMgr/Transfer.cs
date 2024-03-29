﻿using System;
using CLGameServer.Client;
using CLFramework;
namespace CLGameServer
{
    public partial class PlayerMgr
    {
        public void GuildTransferLeaderShip()
        {
            try
            {
                //Create new packet reader
                PacketReader Reader = new PacketReader(PacketInformation.buffer);
                //Read guild id
                int Guildid = Reader.Int32();
                //Read guild member id to transfer to
                int GuildMemberID = Reader.Int32();
                //Close reader
                Reader.Close();

                //Get detailed player information
                PlayerMgr NewLeader = Helpers.GetInformation.GetPlayerid(GuildMemberID);

                //Update database
                DB.query("UPDATE guild_members SET guild_rank='10',guild_perm_join='0',guild_perm_withdraw='0',guild_perm_union='0',guild_perm_storage='0',guild_perm_notice='0' WHERE guild_member_id='" + Character.Information.CharacterID + "'");
                DB.query("UPDATE guild_members SET guild_rank='0',guild_perm_join='1',guild_perm_withdraw='1',guild_perm_union='1',guild_perm_storage='1',guild_perm_notice='1' WHERE guild_member_id='" + GuildMemberID + "'");

                //Repeat for each member in our guild
                foreach (int member in Character.Network.Guild.Members)
                {
                    //Make sure member is not null
                    if (member != 0)
                    {
                        //Get information for the guildmember
                        PlayerMgr guildmember = Helpers.GetInformation.GetPlayerMainid(member);
                        //Make sure the guildmember isnt null
                        if (guildmember != null)
                        {
                            //Send update packet of new leader
                            guildmember.client.Send(Packet.GuildUpdate(Character, 3, GuildMemberID, 0, 0));
                        }
                    }
                }
                //Send message to old owner
                PacketWriter Writer = new PacketWriter();
                //Add opcode
                Writer.Create(OperationCode.SERVER_GUILD_TRANSFER_MSG);
                //Static byte 1
                Writer.Byte(1);
                //Send bytes to client
                client.Send(Writer.GetBytes());
            }
            //If a bad exception error happens
            catch (Exception ex)
            {
                Log.Exception(ex);
            }
        }
    }
}
