﻿using MongoDB.Bson;
using System;

public class Model_Account
{

    public ObjectId _id; 

    public int ActiveConnection { get; set; }
    public string Username{ get; set; }
    public string Discriminator { get; set; }
    public string Email { get; set; }
    public string ShaPassword { get; set; }

    public byte Status { get; set; }
    public string Token { get; set; }
    public DateTime LastLogin { get; set; }

    public Account GetAccount() {
        return new Account() { Username = this.Username, ActiveConnection = this.ActiveConnection, Discriminator = this.Discriminator, Status = this.Status };
    }
}
