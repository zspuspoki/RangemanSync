﻿using System;
using System.Collections.Generic;

namespace Rangeman.Services.WatchDataReceiver
{
    public class PreviousDataTransmitReplayer3
    {
        private readonly IObserver<Tuple<Guid, byte[]>> observer;
        private List<Tuple<Guid, byte[]>> previousData = new List<Tuple<Guid, byte[]>>
        {

new Tuple<Guid, byte[]>( Guid.Parse("26eb0023-b012-49a8-b1f8-394fb2032b0f"), new byte[] { 0x00, 0x0E, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, }), 
new Tuple<Guid, byte[]>( Guid.Parse("26eb0024-b012-49a8-b1f8-394fb2032b0f"), new byte[] { 0x04, 0x00, 0x27, 0x00, 0x27, 0x00, 0x00, 0x00, 0xF4, 0x01, }), 
new Tuple<Guid, byte[]>( Guid.Parse("26eb0024-b012-49a8-b1f8-394fb2032b0f"), new byte[] { 0x02, 0xF0, 0x00, 0x10, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, }), 
new Tuple<Guid, byte[]>( Guid.Parse("26eb0023-b012-49a8-b1f8-394fb2032b0f"), new byte[] { 0x00, 0x10, 0x00, 0xD0, 0x00, 0x00, 0xC0, 0x26, 0x00, 0x00, }), 
new Tuple<Guid, byte[]>( Guid.Parse("26eb0024-b012-49a8-b1f8-394fb2032b0f"), new byte[] { 0x05, 0xFC, 0x1A, 0xF8, 0xF3, 0xEE, 0xF8, 0xDD, 0xE4, 0xF7, 0xB0, 0xC3, 0x59, 0x4E, 0xC7, 0xBB, 0xBF, 0x34, 0xE7, 0x94, 0xC7, 0x52, 0x5B, 0xF2, 0x3F, 0x48, 0xB4, 0xE0, 0xBB, 0x0D, 0xDB, 0x4D, 0xFF, 0xFC, 0x1A, 0xF8, 0xF3, 0xEE, 0xF8, 0xDC, 0xD1, 0x97, 0x6B, 0xB1, 0x42, 0x8C, 0xC7, 0xBB, 0xBF, 0x62, 0x82, 0xB5, 0xA0, 0x10, 0x56, 0xF2, 0x3F, 0xDE, 0xDA, 0xE1, 0xBB, 0x08, 0xDB, 0x50, 0xFF, 0xFC, 0x1A, 0xF8, 0xF3, 0xEE, 0xF8, 0xDC, 0xCB, 0x40, 0x51, 0xDB, 0xDD, 0x8C, 0xC7, 0xBB, 0xBF, 0xA3, 0x40, 0xDE, 0x96, 0x25, 0x56, 0xF2, 0x3F, 0xDF, 0xC3, 0xE1, 0xBB, 0x09, 0xDB, 0x50, 0xFF, 0xFC, 0x1A, 0xF8, 0xF3, 0xEE, 0xF8, 0xD1, 0xCD, 0xB9, 0x9D, 0x26, 0xA0, 0x24, 0xC7, 0xBB, 0xBF, 0x23, 0x04, 0xD2, 0xF3, 0x53, 0x5D, 0xF1, 0x3F, 0x36, 0x2C, 0xE2, 0xBB, 0x07, 0xDB, 0x59, 0xFF, 0xFC, 0x1A, 0xF8, 0xF3, 0xEE, 0xF8, 0xD0, 0xC5, 0xE3, 0x92, 0x6A, 0x79, 0xB8, 0xC8, 0xBB, 0xBF, 0x2E, 0x63, 0x14, 0xD7, 0x7F, 0x4F, 0xF1, 0x3F, 0xA2, 0xE4, 0xCE, 0xBB, 0x28, 0xDB, 0x59, 0xFF, 0xFC, 0x1A, 0xF8, 0xF3, 0xEE, 0xF8, 0xCF, 0xCD, 0x8C, 0x81, 0x37, 0x5E, 0xFE, 0xC9, 0xBB, 0xBF, 0xD7, 0xBA, 0xB5, 0x57, 0x2D, 0x58, 0xF1, 0x3F, 0xF5, 0xE5, 0xCD, 0xBB, 0x2C, 0xDB, 0x59, 0xFF, 0xFC, 0x1A, 0xF8, 0xF3, 0xEE, 0xF8, 0xCE, 0xCE, 0xED, 0xC6, 0x6C, 0x0C, 0x2C, 0xCB, 0xBB, 0xBF, 0xBB, 0xDE, 0xBD, 0xC9, 0x18, 0x6D, 0xF1, 0x3F, 0x20, 0xFF, 0xCC, 0xBB, 0x30, 0xDB, 0x59, 0xFF, 0xFC, 0x1A, 0xF8, 0xF3, 0xEE, 0xF8, 0xCD, 0xCC, 0x94, 0x2F, 0xAD, 0xE4, }), 
new Tuple<Guid, byte[]>( Guid.Parse("26eb0024-b012-49a8-b1f8-394fb2032b0f"), new byte[] { 0x05, 0x5D, 0xCC, 0xBB, 0xBF, 0x56, 0x3C, 0x7A, 0x59, 0x48, 0x59, 0xF1, 0x3F, 0xE2, 0xB8, 0xCA, 0xBB, 0x3B, 0xDB, 0x58, 0xFF, 0x46, 0x3D, }), 
new Tuple<Guid, byte[]>( Guid.Parse("26eb0024-b012-49a8-b1f8-394fb2032b0f"), new byte[] { 0x05, 0xFC, 0x1A, 0xF8, 0xF3, 0xEE, 0xF8, 0xCC, 0xD0, 0x8F, 0x75, 0x96, 0xBF, 0x34, 0xCD, 0xBB, 0xBF, 0x85, 0x3B, 0x63, 0xA6, 0x00, 0x50, 0xF1, 0x3F, 0xA2, 0x56, 0xCA, 0xBB, 0x3C, 0xDB, 0x58, 0xFF, 0xFC, 0x1A, 0xF8, 0xF3, 0xEE, 0xF8, 0xCB, 0xCD, 0x16, 0xBE, 0x77, 0x29, 0x0A, 0xCD, 0xBB, 0xBF, 0x0E, 0x14, 0xAB, 0x67, 0x57, 0x4D, 0xF1, 0x3F, 0x76, 0x18, 0xCC, 0xBB, 0x34, 0xDB, 0x58, 0xFF, 0xFC, 0x1A, 0xF8, 0xF3, 0xEE, 0xF8, 0xCA, 0xCF, 0x56, 0xD6, 0xFD, 0x3C, 0xCF, 0xCC, 0xBB, 0xBF, 0x69, 0x39, 0xE6, 0x3E, 0x9A, 0x48, 0xF1, 0x3F, 0x44, 0x7A, 0xCE, 0xBB, 0x2A, 0xDB, 0x59, 0xFF, 0xFC, 0x1A, 0xF8, 0xF3, 0xEE, 0xF8, 0xC9, 0xCF, 0x98, 0xC3, 0xCB, 0x3C, 0xD1, 0xCC, 0xBB, 0xBF, 0xE9, 0xEF, 0xF9, 0x6A, 0xEA, 0x49, 0xF1, 0x3F, 0x8D, 0x82, 0xCE, 0xBB, 0x2A, 0xDB, 0x5B, 0xFF, 0xFC, 0x1A, 0xF8, 0xF3, 0xEE, 0xF8, 0xC8, 0xD0, 0x5F, 0xB9, 0xF7, 0xF3, 0xCB, 0xCC, 0xBB, 0xBF, 0x81, 0x8F, 0xAD, 0x7B, 0xC5, 0x49, 0xF1, 0x3F, 0xFC, 0x7D, 0xCE, 0xBB, 0x2A, 0xDB, 0x5F, 0xFF, 0xFC, 0x1A, 0xF8, 0xF3, 0xEE, 0xF8, 0xC4, 0xF1, 0xE8, 0xFF, 0x66, 0x73, 0xD0, 0xCC, 0xBB, 0xBF, 0xFA, 0x7B, 0x00, 0x65, 0x08, 0x4A, 0xF1, 0x3F, 0x02, 0x0C, 0xCE, 0xBB, 0x2C, 0xDB, 0x76, 0xFF, 0xFD, 0x1A, 0xF8, 0xF3, 0xEE, 0xF7, 0xFF, 0xDE, 0xDA, 0xD8, 0xA1, 0x6D, 0xCD, 0xCC, 0xBB, 0xBF, 0x2E, 0x56, 0x0B, 0xAA, 0x5C, 0x4B, 0xF1, 0x3F, 0xD1, 0x68, 0xCD, 0xBB, 0x2F, 0xDB, 0x7F, 0xFF, 0xFC, 0x1A, 0xF8, 0xF3, 0xEE, 0xF6, 0xEE, 0xE6, 0x95, 0x06, 0x22, 0x51, }), 
new Tuple<Guid, byte[]>( Guid.Parse("26eb0024-b012-49a8-b1f8-394fb2032b0f"), new byte[] { 0x05, 0xCD, 0xCC, 0xBB, 0xBF, 0x6F, 0xCD, 0x12, 0x43, 0x7C, 0x49, 0xF1, 0x3F, 0x79, 0x58, 0xCF, 0xBB, 0x26, 0xDB, 0x55, 0xFF, 0x30, 0x1C, }), 
new Tuple<Guid, byte[]>( Guid.Parse("26eb0024-b012-49a8-b1f8-394fb2032b0f"), new byte[] { 0x05, 0xFC, 0x1A, 0xF8, 0xF3, 0xEE, 0xF6, 0xEE, 0xC4, 0xAB, 0x25, 0xA2, 0xC2, 0xCD, 0xCC, 0xBB, 0xBF, 0xB3, 0x0E, 0xB1, 0x40, 0x88, 0x49, 0xF1, 0x3F, 0x07, 0x45, 0xCF, 0xBB, 0x26, 0xDB, 0x59, 0xFF, 0xFC, 0x1A, 0xF8, 0xF3, 0xEE, 0xF6, 0xED, 0xF6, 0xC4, 0x6F, 0xE4, 0x70, 0xCD, 0xCC, 0xBB, 0xBF, 0x73, 0x25, 0xFF, 0xD0, 0x90, 0x49, 0xF1, 0x3F, 0x9A, 0x3E, 0xCF, 0xBB, 0x26, 0xDB, 0x5A, 0xFF, 0xFC, 0x1A, 0xF8, 0xF3, 0xEE, 0xF6, 0xEC, 0xE2, 0xD2, 0xC5, 0x8B, 0xF9, 0xD0, 0xCC, 0xBB, 0xBF, 0x67, 0x2B, 0x74, 0x33, 0x7D, 0x4A, 0xF1, 0x3F, 0x91, 0x19, 0xCF, 0xBB, 0x27, 0xDB, 0x5F, 0xFF, 0xFC, 0x1A, 0xF8, 0xF3, 0xEE, 0xF6, 0xE9, 0xCB, 0x52, 0xD5, 0x26, 0x64, 0xCD, 0xCC, 0xBB, 0xBF, 0x05, 0x5D, 0x1F, 0x5B, 0xCF, 0x4D, 0xF1, 0x3F, 0xB3, 0xF6, 0xCD, 0xBB, 0x2C, 0xDB, 0x66, 0xFF, 0xFC, 0x1A, 0xF8, 0xF3, 0xEE, 0xF6, 0xE8, 0xEE, 0xF3, 0x66, 0x21, 0x2F, 0xCE, 0xCC, 0xBB, 0xBF, 0xB9, 0x79, 0x96, 0xBC, 0x0C, 0x4E, 0xF1, 0x3F, 0x0E, 0x34, 0xCD, 0xBB, 0x2F, 0xDB, 0x67, 0xFF, 0xFC, 0x1A, 0xF8, 0xF3, 0xEE, 0xF6, 0xE8, 0xE9, 0x94, 0xEA, 0x75, 0x29, 0xD0, 0xCC, 0xBB, 0xBF, 0xFB, 0xE1, 0x40, 0x7D, 0x26, 0x4E, 0xF1, 0x3F, 0x60, 0x4A, 0xCD, 0xBB, 0x2F, 0xDB, 0x67, 0xFF, 0xFC, 0x1A, 0xF8, 0xF3, 0xEE, 0xF6, 0xE7, 0xEA, 0x2B, 0x33, 0x96, 0xDB, 0xD3, 0xCC, 0xBB, 0xBF, 0x8F, 0x75, 0xAD, 0xA0, 0x57, 0x4F, 0xF1, 0x3F, 0xFE, 0xF5, 0xCC, 0xBB, 0x31, 0xDB, 0x6A, 0xFF, 0xFC, 0x1A, 0xF8, 0xF3, 0xEE, 0xF6, 0xE6, 0xED, 0xAA, 0xB1, 0xEF, 0xB5, }), 
new Tuple<Guid, byte[]>( Guid.Parse("26eb0024-b012-49a8-b1f8-394fb2032b0f"), new byte[] { 0x05, 0xDE, 0xCC, 0xBB, 0xBF, 0xBF, 0xA5, 0x7B, 0x96, 0xDC, 0x4F, 0xF1, 0x3F, 0x87, 0x62, 0xCC, 0xBB, 0x33, 0xDB, 0x6C, 0xFF, 0xB7, 0xDB, }), 
new Tuple<Guid, byte[]>( Guid.Parse("26eb0024-b012-49a8-b1f8-394fb2032b0f"), new byte[] { 0x05, 0xFC, 0x1A, 0xF8, 0xF3, 0xEE, 0xF6, 0xE6, 0xD3, 0x30, 0x32, 0xF9, 0x35, 0xDD, 0xCC, 0xBB, 0xBF, 0xDA, 0x43, 0x5E, 0xC1, 0x7D, 0x4F, 0xF1, 0x3F, 0xCD, 0x52, 0xCC, 0xBB, 0x33, 0xDB, 0x6D, 0xFF, 0xFC, 0x1A, 0xF8, 0xF3, 0xEE, 0xF6, 0xE5, 0xFE, 0xD7, 0x02, 0x2D, 0x28, 0xE0, 0xCC, 0xBB, 0xBF, 0x15, 0x68, 0x18, 0xA6, 0x67, 0x4F, 0xF1, 0x3F, 0xEA, 0x37, 0xCC, 0xBB, 0x34, 0xDB, 0x6D, 0xFF, 0xFC, 0x1A, 0xF8, 0xF3, 0xEE, 0xF6, 0xE5, 0xE4, 0x42, 0x70, 0xD7, 0xAC, 0xE2, 0xCC, 0xBB, 0xBF, 0x71, 0x9C, 0x08, 0xB4, 0xB2, 0x4F, 0xF1, 0x3F, 0x11, 0x37, 0xCC, 0xBB, 0x34, 0xDB, 0x6E, 0xFF, 0xFC, 0x1A, 0xF8, 0xF3, 0xEE, 0xF6, 0xE4, 0xDB, 0x6D, 0x00, 0x97, 0x1B, 0xFD, 0xCC, 0xBB, 0xBF, 0xFE, 0x88, 0xF1, 0xD9, 0xE3, 0x50, 0xF1, 0x3F, 0x40, 0x68, 0xCB, 0xBB, 0x38, 0xDB, 0x6A, 0xFF, 0xFC, 0x1A, 0xF8, 0xF3, 0xEE, 0xF6, 0xE3, 0xE0, 0xC3, 0xE9, 0x2A, 0x36, 0x1B, 0xCD, 0xBB, 0xBF, 0xC0, 0x50, 0xB1, 0x55, 0x84, 0x50, 0xF1, 0x3F, 0x65, 0x72, 0xCB, 0xBB, 0x37, 0xDB, 0x67, 0xFF, 0xFC, 0x1A, 0xF8, 0xF3, 0xEE, 0xF6, 0xE2, 0xE9, 0x44, 0x57, 0x72, 0x5D, 0x21, 0xCD, 0xBB, 0xBF, 0x55, 0x85, 0xB2, 0x71, 0x4C, 0x47, 0xF1, 0x3F, 0xDB, 0x86, 0xCB, 0xBB, 0x37, 0xDB, 0x62, 0xFF, 0xFC, 0x1A, 0xF8, 0xF3, 0xEE, 0xF6, 0xE0, 0xE7, 0x41, 0xBE, 0x88, 0xA0, 0xCE, 0xCC, 0xBB, 0xBF, 0xB8, 0x4D, 0x63, 0x3C, 0x58, 0x17, 0xF1, 0x3F, 0x5A, 0x51, 0xD4, 0xBB, 0x0F, 0xDB, 0x57, 0xFF, 0xFC, 0x1A, 0xF8, 0xF3, 0xEE, 0xF6, 0xDF, 0xE7, 0xE2, 0x81, 0xC8, 0x9B, }), 
new Tuple<Guid, byte[]>( Guid.Parse("26eb0024-b012-49a8-b1f8-394fb2032b0f"), new byte[] { 0x05, 0x03, 0xCD, 0xBB, 0xBF, 0xB5, 0x49, 0xEC, 0xBD, 0x3F, 0xF5, 0xF0, 0x3F, 0x88, 0x3D, 0xDE, 0xBB, 0xE2, 0xDA, 0x53, 0xFF, 0x82, 0xAA, }), 
new Tuple<Guid, byte[]>( Guid.Parse("26eb0024-b012-49a8-b1f8-394fb2032b0f"), new byte[] { 0x05, 0xFC, 0x1A, 0xF8, 0xF3, 0xEE, 0xF6, 0xDE, 0xE6, 0x77, 0x7C, 0xED, 0x65, 0xD9, 0xCD, 0xBB, 0xBF, 0x1B, 0x79, 0xF4, 0x2F, 0x61, 0xD4, 0xF0, 0x3F, 0x16, 0x24, 0xE1, 0xBB, 0xD5, 0xDA, 0x4E, 0xFF, 0xFC, 0x1A, 0xF8, 0xF3, 0xEE, 0xF6, 0xDD, 0xE8, 0xBD, 0x51, 0xA0, 0x3E, 0x3B, 0xCF, 0xBB, 0xBF, 0xF9, 0x36, 0x52, 0x94, 0xEF, 0xBE, 0xF0, 0x3F, 0x4F, 0x55, 0xE7, 0xBB, 0xB8, 0xDA, 0x46, 0xFF, 0xFC, 0x1A, 0xF8, 0xF3, 0xEE, 0xF6, 0xDC, 0xE2, 0x67, 0xBF, 0xE7, 0x93, 0xE5, 0xD0, 0xBB, 0xBF, 0xCE, 0xEA, 0x17, 0x3E, 0x7A, 0xA7, 0xF0, 0x3F, 0x1C, 0x92, 0xEA, 0xBB, 0xAA, 0xDA, 0x3C, 0xFF, 0xFC, 0x1A, 0xF8, 0xF3, 0xEE, 0xF6, 0xDB, 0xE5, 0xFF, 0x39, 0x09, 0x6D, 0xAE, 0xD1, 0xBB, 0xBF, 0x8D, 0xA4, 0xAD, 0x28, 0x8C, 0x87, 0xF0, 0x3F, 0xE5, 0xB6, 0xED, 0xBB, 0x9B, 0xDA, 0x33, 0xFF, 0xFC, 0x1A, 0xF8, 0xF3, 0xEE, 0xF6, 0xDA, 0xE5, 0x7A, 0xB1, 0x02, 0x1E, 0xF3, 0xD0, 0xBB, 0xBF, 0x4F, 0xAF, 0x9F, 0xF1, 0x0C, 0x6C, 0xF0, 0x3F, 0xE5, 0x21, 0xF2, 0xBB, 0x87, 0xDA, 0x2D, 0xFF, 0xFC, 0x1A, 0xF8, 0xF3, 0xEE, 0xF6, 0xD9, 0xE9, 0xDC, 0xED, 0x5B, 0xFB, 0xAE, 0xCF, 0xBB, 0xBF, 0x6A, 0x41, 0xC7, 0xE9, 0x43, 0x59, 0xF0, 0x3F, 0x8D, 0x59, 0xED, 0xBB, 0x9D, 0xDA, 0x2A, 0xFF, 0xFC, 0x1A, 0xF8, 0xF3, 0xEE, 0xF6, 0xD8, 0xEB, 0x09, 0x4B, 0x98, 0xB2, 0xDD, 0xCE, 0xBB, 0xBF, 0x6C, 0xFF, 0x2C, 0xF2, 0xFF, 0x3C, 0xF0, 0x3F, 0xE2, 0x63, 0xE8, 0xBB, 0xB4, 0xDA, 0x27, 0xFF, 0xFC, 0x1A, 0xF8, 0xF3, 0xEE, 0xF6, 0xD7, 0xEA, 0x81, 0x05, 0xD2, 0x18, }), 
new Tuple<Guid, byte[]>( Guid.Parse("26eb0024-b012-49a8-b1f8-394fb2032b0f"), new byte[] { 0x05, 0x19, 0xCE, 0xBB, 0xBF, 0xB6, 0xD7, 0xBA, 0xEB, 0x31, 0x20, 0xF0, 0x3F, 0x78, 0x53, 0xE2, 0xBB, 0xCF, 0xDA, 0x24, 0xFF, 0xB7, 0x31, }), 
new Tuple<Guid, byte[]>( Guid.Parse("26eb0024-b012-49a8-b1f8-394fb2032b0f"), new byte[] { 0x05, 0xFC, 0x1A, 0xF8, 0xF3, 0xEE, 0xF6, 0xD6, 0xE9, 0x10, 0x89, 0xE9, 0x56, 0x61, 0xCD, 0xBB, 0xBF, 0x9C, 0x47, 0x60, 0x37, 0x40, 0x0F, 0xF0, 0x3F, 0x1C, 0x6F, 0xDF, 0xBB, 0xDD, 0xDA, 0x22, 0xFF, 0xFC, 0x1A, 0xF8, 0xF3, 0xEE, 0xF6, 0xD5, 0xD9, 0x76, 0x8E, 0x31, 0x94, 0x73, 0xCC, 0xBB, 0xBF, 0x94, 0xB7, 0xB3, 0x35, 0x5B, 0x0B, 0xF0, 0x3F, 0x13, 0x5D, 0xE1, 0xBB, 0xD4, 0xDA, 0x21, 0xFF, 0xFC, 0x1A, 0xF8, 0xF3, 0xEE, 0xF6, 0xD4, 0xE6, 0xF0, 0x5E, 0x33, 0x83, 0x40, 0xCC, 0xBB, 0xBF, 0xD6, 0xD3, 0xD7, 0x2B, 0x08, 0x08, 0xF0, 0x3F, 0xD3, 0xA3, 0xE3, 0xBB, 0xC9, 0xDA, 0x22, 0xFF, 0xFC, 0x1A, 0xF8, 0xF3, 0xEE, 0xF6, 0xD3, 0xF8, 0xD9, 0xBE, 0x25, 0xF6, 0x3B, 0xCC, 0xBB, 0xBF, 0x9A, 0x07, 0x08, 0x3E, 0x63, 0x06, 0xF0, 0x3F, 0x9A, 0x96, 0xE2, 0xBB, 0xCE, 0xDA, 0x22, 0xFF, 0xFC, 0x1A, 0xF8, 0xF3, 0xEE, 0xF6, 0xD3, 0xEF, 0x3A, 0xDA, 0x7A, 0x7E, 0x3B, 0xCC, 0xBB, 0xBF, 0x3D, 0x52, 0x76, 0x38, 0x6E, 0x06, 0xF0, 0x3F, 0xBC, 0x9F, 0xE2, 0xBB, 0xCE, 0xDA, 0x22, 0xFF, 0xFC, 0x1A, 0xF8, 0xF3, 0xEE, 0xF6, 0xD2, 0xEA, 0xA2, 0x39, 0xED, 0xA9, 0x30, 0xCC, 0xBB, 0xBF, 0x63, 0xB9, 0x70, 0x34, 0xCF, 0x05, 0xF0, 0x3F, 0xBD, 0x43, 0xE2, 0xBB, 0xD0, 0xDA, 0x2C, 0xFF, 0xFC, 0x1A, 0xF8, 0xF3, 0xEE, 0xF6, 0xD1, 0xE6, 0x05, 0x50, 0x7B, 0xC7, 0x23, 0xCC, 0xBB, 0xBF, 0xBB, 0x79, 0xB1, 0x90, 0xAD, 0x04, 0xF0, 0x3F, 0xE9, 0xC4, 0xE1, 0xBB, 0xD2, 0xDA, 0x35, 0xFF, 0xFC, 0x1A, 0xF8, 0xF3, 0xEE, 0xF6, 0xD0, 0xEC, 0x51, 0x4B, 0x72, 0x66, }), 
new Tuple<Guid, byte[]>( Guid.Parse("26eb0024-b012-49a8-b1f8-394fb2032b0f"), new byte[] { 0x05, 0x21, 0xCC, 0xBB, 0xBF, 0xB6, 0xDA, 0x20, 0x53, 0x45, 0x04, 0xF0, 0x3F, 0x12, 0x3A, 0xE1, 0xBB, 0xD4, 0xDA, 0x3A, 0xFF, 0xB3, 0x5B, }), 

new Tuple<Guid, byte[]>( Guid.Parse("26eb0024-b012-49a8-b1f8-394fb2032b0f"), new byte[] { 0x05, 0xFC, 0x1A, 0xF8, 0xF3, 0xEE, 0xF6, 0xCF, 0xEC, 0x31, 0x5B, 0x15, 0xD8, 0x1D, 0xCC, 0xBB, 0xBF, 0x59, 0x54, 0x0A, 0x44, 0xF5, 0x03, 0xF0, 0x3F, 0xCA, 0x3D, 0xE1, 0xBB, 0xD4, 0xDA, 0x3E, 0xFF, 0xFC, 0x1A, 0xF8, 0xF3, 0xEE, 0xF6, 0xCE, 0xEC, 0x0E, 0xE1, 0x0C, 0x49, 0x24, 0xCC, 0xBB, 0xBF, 0x4D, 0xDB, 0x98, 0x2C, 0x38, 0x04, 0xF0, 0x3F, 0xA5, 0x33, 0xE1, 0xBB, 0xD4, 0xDA, 0x43, 0xFF, 0xFC, 0x1A, 0xF8, 0xF3, 0xEE, 0xF6, 0xCD, 0xEC, 0x5D, 0xC5, 0x90, 0x47, 0x21, 0xCC, 0xBB, 0xBF, 0x7F, 0x59, 0xF8, 0x14, 0x45, 0x04, 0xF0, 0x3F, 0x16, 0x3B, 0xE1, 0xBB, 0xD4, 0xDA, 0x49, 0xFF, 0xFC, 0x1A, 0xF8, 0xF3, 0xEE, 0xF6, 0xCC, 0xEC, 0xF6, 0xB5, 0xE4, 0xF1, 0x1F, 0xCC, 0xBB, 0xBF, 0xE1, 0x51, 0x69, 0x63, 0xFD, 0x03, 0xF0, 0x3F, 0xC5, 0x47, 0xE1, 0xBB, 0xD4, 0xDA, 0x4F, 0xFF, 0xFC, 0x1A, 0xF8, 0xF3, 0xEE, 0xF6, 0xCB, 0xEC, 0xF2, 0x22, 0xDC, 0x2E, 0x24, 0xCC, 0xBB, 0xBF, 0x9B, 0xC5, 0xA6, 0xEC, 0x30, 0x04, 0xF0, 0x3F, 0x12, 0x51, 0xE1, 0xBB, 0xD4, 0xDA, 0x56, 0xFF, 0xFC, 0x1A, 0xF8, 0xF3, 0xEE, 0xF6, 0xCA, 0xF3, 0xC0, 0x8C, 0x76, 0xDA, 0x1E, 0xCC, 0xBB, 0xBF, 0x0F, 0x7C, 0x4A, 0xF4, 0x20, 0x04, 0xF0, 0x3F, 0x7F, 0x57, 0xE1, 0xBB, 0xD4, 0xDA, 0x5A, 0xFF, 0xFC, 0x1A, 0xF8, 0xF3, 0xEE, 0xF6, 0xCA, 0xEF, 0xC5, 0xB6, 0x9A, 0x12, 0x1F, 0xCC, 0xBB, 0xBF, 0xCB, 0xFA, 0x40, 0x37, 0x20, 0x04, 0xF0, 0x3F, 0x7F, 0x57, 0xE1, 0xBB, 0xD4, 0xDA, 0x5A, 0xFF, 0xFC, 0x1A, 0xF8, 0xF3, 0xEE, 0xF6, 0xC9, 0xE7, 0x3F, 0xBD, 0xE9, 0x0F, }), 

new Tuple<Guid, byte[]>( Guid.Parse("26eb0024-b012-49a8-b1f8-394fb2032b0f"), new byte[] { 0x05, 0x21, 0xCC, 0xBB, 0xBF, 0xF3, 0x73, 0x7A, 0xFC, 0x33, 0x04, 0xF0, 0x3F, 0xC6, 0x53, 0xE1, 0xBB, 0xD4, 0xDA, 0x61, 0xFF, 0xF0, 0xEB, }), 

new Tuple<Guid, byte[]>( Guid.Parse("26eb0024-b012-49a8-b1f8-394fb2032b0f"), new byte[] { 0x05, 0xFC, 0x1A, 0xF8, 0xF3, 0xEE, 0xF6, 0xC8, 0xE5, 0x7A, 0xFB, 0x98, 0xB1, 0x1B, 0xCC, 0xBB, 0xBF, 0x07, 0xBC, 0xBF, 0x13, 0xF8, 0x03, 0xF0, 0x3F, 0x7D, 0x4B, 0xE1, 0xBB, 0xD4, 0xDA, 0x69, 0xFF, 0xFC, 0x1A, 0xF8, 0xF3, 0xEE, 0xF6, 0xC7, 0xED, 0xD0, 0x74, 0x73, 0x0C, 0x19, 0xCC, 0xBB, 0xBF, 0x19, 0x96, 0x9F, 0x28, 0xAD, 0x03, 0xF0, 0x3F, 0xCA, 0x54, 0xE1, 0xBB, 0xD4, 0xDA, 0x6D, 0xFF, 0xFC, 0x1A, 0xF8, 0xF3, 0xEE, 0xF6, 0xC6, 0xE8, 0x09, 0x37, 0x24, 0x67, 0x1B, 0xCC, 0xBB, 0xBF, 0xEF, 0x8B, 0xA1, 0xF4, 0xAE, 0x03, 0xF0, 0x3F, 0x12, 0x51, 0xE1, 0xBB, 0xD4, 0xDA, 0x70, 0xFF, 0xFC, 0x1A, 0xF8, 0xF3, 0xEE, 0xF6, 0xC5, 0xFE, 0x41, 0x5B, 0x0C, 0xF6, 0x1A, 0xCC, 0xBB, 0xBF, 0xF2, 0x61, 0x31, 0x01, 0xD5, 0x03, 0xF0, 0x3F, 0xEE, 0x3B, 0xE1, 0xBB, 0xD4, 0xDA, 0x70, 0xFF, 0xFC, 0x1A, 0xF8, 0xF3, 0xEE, 0xF6, 0xC5, 0xEB, 0x42, 0x03, 0x8C, 0x81, 0x15, 0xCC, 0xBB, 0xBF, 0x97, 0x06, 0x7B, 0x22, 0xDA, 0x03, 0xF0, 0x3F, 0x5F, 0x43, 0xE1, 0xBB, 0xD4, 0xDA, 0x71, 0xFF, 0xFC, 0x1A, 0xF8, 0xF3, 0xEE, 0xF6, 0xC4, 0xE9, 0x74, 0xF8, 0x52, 0x4B, 0x18, 0xCC, 0xBB, 0xBF, 0x8E, 0x30, 0x07, 0xB0, 0x47, 0x03, 0xF0, 0x3F, 0x40, 0x82, 0xE0, 0xBB, 0xD8, 0xDA, 0x71, 0xFF, 0xFD, 0x1A, 0xF8, 0xF3, 0xEE, 0xF5, 0xFF, 0xD9, 0x94, 0x3C, 0x2A, 0x3A, 0x21, 0xCC, 0xBB, 0xBF, 0x10, 0x67, 0x0E, 0x3A, 0x0D, 0x04, 0xF0, 0x3F, 0xCF, 0x91, 0xE0, 0xBB, 0xD7, 0xDA, 0x72, 0xFF, 0xFD, 0x1A, 0xF8, 0xF3, 0xEE, 0xF5, 0xFE, 0xDF, 0x04, 0x30, 0x1A, 0xD1, }), 

new Tuple<Guid, byte[]>( Guid.Parse("26eb0024-b012-49a8-b1f8-394fb2032b0f"), new byte[] { 0x05, 0x18, 0xCC, 0xBB, 0xBF, 0xD9, 0xCD, 0x6F, 0x08, 0xD2, 0x03, 0xF0, 0x3F, 0xAF, 0x94, 0xE0, 0xBB, 0xD7, 0xDA, 0x72, 0xFF, 0x5C, 0x96, }), 

new Tuple<Guid, byte[]>( Guid.Parse("26eb0024-b012-49a8-b1f8-394fb2032b0f"), new byte[] { 0x05, 0xFD, 0x1A, 0xF8, 0xF3, 0xEE, 0xF5, 0xFE, 0xDB, 0x45, 0xDE, 0xEE, 0x2C, 0x19, 0xCC, 0xBB, 0xBF, 0xF3, 0xB5, 0x47, 0x70, 0xD4, 0x03, 0xF0, 0x3F, 0x3C, 0x98, 0xE0, 0xBB, 0xD7, 0xDA, 0x72, 0xFF, 0xFD, 0x1A, 0xF8, 0xF3, 0xEE, 0xF5, 0xFD, 0xDA, 0xF3, 0x8A, 0x24, 0x92, 0x1C, 0xCC, 0xBB, 0xBF, 0x4D, 0x8D, 0xE1, 0x39, 0xEA, 0x03, 0xF0, 0x3F, 0xCD, 0x9C, 0xE0, 0xBB, 0xD7, 0xDA, 0x75, 0xFF, 0xFD, 0x1A, 0xF8, 0xF3, 0xEE, 0xF5, 0xFC, 0xE0, 0x5E, 0xA2, 0xCB, 0x75, 0x1B, 0xCC, 0xBB, 0xBF, 0x62, 0xDD, 0x3A, 0x50, 0xDC, 0x03, 0xF0, 0x3F, 0x16, 0xA5, 0xE0, 0xBB, 0xD7, 0xDA, 0x78, 0xFF, 0xFD, 0x1A, 0xF8, 0xF3, 0xEE, 0xF5, 0xFA, 0xD9, 0x5C, 0xC5, 0x1B, 0xDC, 0x24, 0xCC, 0xBB, 0xBF, 0xC8, 0xF1, 0xC4, 0xC8, 0x10, 0x03, 0xF0, 0x3F, 0x85, 0xA0, 0xE0, 0xBB, 0xD7, 0xDA, 0x81, 0xFF, 0xFD, 0x1A, 0xF8, 0xF3, 0xEE, 0xF5, 0xF9, 0xFD, 0x06, 0xE7, 0xB6, 0x61, 0x22, 0xCC, 0xBB, 0xBF, 0xF7, 0x91, 0xA2, 0x0C, 0xC5, 0x02, 0xF0, 0x3F, 0x18, 0xB1, 0xE0, 0xBB, 0xD7, 0xDA, 0x81, 0xFF, 0xFD, 0x1A, 0xF8, 0xF3, 0xEE, 0xF5, 0xF9, 0xEE, 0x04, 0xC8, 0xA1, 0x95, 0x1F, 0xCC, 0xBB, 0xBF, 0x52, 0x74, 0xB1, 0x86, 0xCA, 0x02, 0xF0, 0x3F, 0xAC, 0xB6, 0xE0, 0xBB, 0xD7, 0xDA, 0x81, 0xFF, 0xFD, 0x1A, 0xF8, 0xF3, 0xEE, 0xF5, 0xF8, 0xDF, 0x9D, 0xB6, 0xA2, 0x03, 0x1B, 0xCC, 0xBB, 0xBF, 0x07, 0x3C, 0x3A, 0xC4, 0xAA, 0x02, 0xF0, 0x3F, 0xA7, 0xA9, 0xE0, 0xBB, 0xD7, 0xDA, 0x86, 0xFF, 0xFD, 0x1A, 0xF8, 0xF3, 0xEE, 0xF5, 0xF7, 0xDD, 0x9D, 0xAA, 0xD0, 0xC2, }), 

new Tuple<Guid, byte[]>( Guid.Parse("26eb0024-b012-49a8-b1f8-394fb2032b0f"), new byte[] { 0x05, 0x21, 0xCC, 0xBB, 0xBF, 0x22, 0x8D, 0x5D, 0xE4, 0xA8, 0x02, 0xF0, 0x3F, 0x61, 0xA2, 0xE0, 0xBB, 0xD7, 0xDA, 0x8A, 0xFF, 0xA0, 0xF9, }), 

new Tuple<Guid, byte[]>( Guid.Parse("26eb0024-b012-49a8-b1f8-394fb2032b0f"), new byte[] { 0x05, 0xFD, 0x1A, 0xF8, 0xF3, 0xEE, 0xF5, 0xF7, 0xD9, 0xEC, 0x7F, 0xF7, 0xFC, 0x21, 0xCC, 0xBB, 0xBF, 0xAD, 0x66, 0x3C, 0xF4, 0xAC, 0x02, 0xF0, 0x3F, 0x85, 0xA0, 0xE0, 0xBB, 0xD7, 0xDA, 0x8A, 0xFF, 0xFD, 0x1A, 0xF8, 0xF3, 0xEE, 0xF5, 0xF6, 0xDB, 0xB3, 0xB2, 0x92, 0xBB, 0x25, 0xCC, 0xBB, 0xBF, 0x4A, 0x3F, 0x0A, 0xCD, 0xC0, 0x02, 0xF0, 0x3F, 0x3A, 0xA3, 0xE0, 0xBB, 0xD7, 0xDA, 0x8C, 0xFF, 0xFD, 0x1A, 0xF8, 0xF3, 0xEE, 0xF5, 0xF6, 0xC7, 0x17, 0xBA, 0x02, 0x58, 0x24, 0xCC, 0xBB, 0xBF, 0x29, 0x8D, 0xC5, 0xBD, 0x2F, 0x03, 0xF0, 0x3F, 0xA9, 0x34, 0xE1, 0xBB, 0xD4, 0xDA, 0x8D, 0xFF, 0xFD, 0x1A, 0xF8, 0xF3, 0xEE, 0xF5, 0xF5, 0xD9, 0x04, 0x2D, 0xDA, 0xF2, 0x2C, 0xCC, 0xBB, 0xBF, 0x3E, 0x1C, 0x40, 0x2B, 0x09, 0x04, 0xF0, 0x3F, 0x33, 0x88, 0xE1, 0xBB, 0xD3, 0xDA, 0x8E, 0xFF, 0xFD, 0x1A, 0xF8, 0xF3, 0xEE, 0xF5, 0xF4, 0xE6, 0x01, 0xBA, 0x7F, 0xD4, 0x2A, 0xCC, 0xBB, 0xBF, 0x08, 0x43, 0xCC, 0x51, 0xC3, 0x04, 0xF0, 0x3F, 0x9F, 0xBC, 0xE1, 0xBB, 0xD2, 0xDA, 0x90, 0xFF, 0xFD, 0x1A, 0xF8, 0xF3, 0xEE, 0xF5, 0xF3, 0xEF, 0x79, 0x05, 0xB2, 0xC7, 0x33, 0xCC, 0xBB, 0xBF, 0xAA, 0x03, 0x80, 0xAE, 0x0F, 0x05, 0xF0, 0x3F, 0x9E, 0x2F, 0xE2, 0xBB, 0xD0, 0xDA, 0x91, 0xFF, 0xFD, 0x1A, 0xF8, 0xF3, 0xEE, 0xF5, 0xF3, 0xD9, 0x69, 0x44, 0x62, 0xBA, 0x36, 0xCC, 0xBB, 0xBF, 0x6F, 0x47, 0xDF, 0xF5, 0x93, 0x05, 0xF0, 0x3F, 0x08, 0x58, 0xE2, 0xBB, 0xCF, 0xDA, 0x91, 0xFF, 0xFD, 0x1A, 0xF8, 0xF3, 0xEE, 0xF5, 0xF2, 0xDE, 0x14, 0x92, 0x0C, 0x60, }), 

new Tuple<Guid, byte[]>( Guid.Parse("26eb0024-b012-49a8-b1f8-394fb2032b0f"), new byte[] { 0x05, 0x40, 0xCC, 0xBB, 0xBF, 0xF3, 0xA5, 0x78, 0x6A, 0x6D, 0x06, 0xF0, 0x3F, 0x4E, 0xC7, 0xE2, 0xBB, 0xCD, 0xDA, 0x92, 0xFF, 0xFE, 0xB5, }), 

new Tuple<Guid, byte[]>( Guid.Parse("26eb0024-b012-49a8-b1f8-394fb2032b0f"), new byte[] { 0x05, 0xFD, 0x1A, 0xF8, 0xF3, 0xEE, 0xF5, 0xF1, 0xE9, 0x53, 0xD2, 0x76, 0x9D, 0x49, 0xCC, 0xBB, 0xBF, 0xAA, 0xCC, 0x0D, 0x43, 0x6E, 0x06, 0xF0, 0x3F, 0xBA, 0x93, 0xE2, 0xBB, 0xCE, 0xDA, 0x92, 0xFF, 0xFD, 0x1A, 0xF8, 0xF3, 0xEE, 0xF5, 0xF0, 0xE5, 0xC6, 0x3F, 0x26, 0x16, 0x41, 0xCC, 0xBB, 0xBF, 0xF6, 0x88, 0x5E, 0xC9, 0xA9, 0x04, 0xF0, 0x3F, 0xA3, 0xD4, 0xE1, 0xBB, 0xD2, 0xDA, 0x90, 0xFF, 0xFD, 0x1A, 0xF8, 0xF3, 0xEE, 0xF5, 0xEF, 0xEF, 0xE8, 0x6A, 0x50, 0xF9, 0xF5, 0xCB, 0xBB, 0xBF, 0x0A, 0xE3, 0xEF, 0xE8, 0x7D, 0x05, 0xF0, 0x3F, 0x61, 0xD0, 0xE0, 0xBB, 0xD6, 0xDA, 0x8D, 0xFF, 0xFD, 0x1A, 0xF8, 0xF3, 0xEE, 0xF5, 0xEE, 0xEF, 0x5B, 0x3F, 0xC1, 0xD1, 0xD0, 0xCB, 0xBB, 0xBF, 0x92, 0x23, 0x84, 0x24, 0xDC, 0x05, 0xF0, 0x3F, 0x3F, 0x1A, 0xE0, 0xBB, 0xDA, 0xDA, 0x89, 0xFF, 0xFD, 0x1A, 0xF8, 0xF3, 0xEE, 0xF5, 0xEE, 0xDC, 0x46, 0xBA, 0xBC, 0xB2, 0xB4, 0xCB, 0xBB, 0xBF, 0xFB, 0x19, 0xF8, 0xB7, 0x1A, 0x03, 0xF0, 0x3F, 0xB0, 0x0C, 0xDF, 0xBB, 0xDE, 0xDA, 0x88, 0xFF, 0xFD, 0x1A, 0xF8, 0xF3, 0xEE, 0xF5, 0xED, 0xEE, 0x59, 0x7C, 0x9C, 0x75, 0x39, 0xCB, 0xBB, 0xBF, 0xCD, 0xAC, 0xA8, 0x69, 0x4F, 0xFF, 0xEF, 0x3F, 0xD2, 0x2A, 0xE0, 0xBB, 0xD9, 0xDA, 0x85, 0xFF, 0xFC, 0x1A, 0xF8, 0xF3, 0xEE, 0xF5, 0xEC, 0xE1, 0x9E, 0xC4, 0xC1, 0x1E, 0xF3, 0xC9, 0xBB, 0xBF, 0x17, 0xAA, 0x10, 0x60, 0x2C, 0xFD, 0xEF, 0x3F, 0x40, 0xD7, 0xDE, 0xBB, 0xDF, 0xDA, 0x7F, 0xFF, 0xFC, 0x1A, 0xF8, 0xF3, 0xEE, 0xF5, 0xEB, 0xDE, 0xB7, 0xAB, 0x8E, 0x0A, }), 

new Tuple<Guid, byte[]>( Guid.Parse("26eb0024-b012-49a8-b1f8-394fb2032b0f"), new byte[] { 0x05, 0xAA, 0xC8, 0xBB, 0xBF, 0xD0, 0x25, 0x9E, 0xAD, 0xA8, 0xFD, 0xEF, 0x3F, 0xD1, 0xDB, 0xDE, 0xBB, 0xDF, 0xDA, 0x7B, 0xFF, 0x25, 0x07, }), 

new Tuple<Guid, byte[]>( Guid.Parse("26eb0024-b012-49a8-b1f8-394fb2032b0f"), new byte[] { 0x05, 0xFC, 0x1A, 0xF8, 0xF3, 0xEE, 0xF5, 0xEA, 0xE2, 0x9F, 0x70, 0xD1, 0x67, 0xBB, 0xC7, 0xBB, 0xBF, 0xD0, 0xA5, 0x98, 0x44, 0x2E, 0xFD, 0xEF, 0x3F, 0x1C, 0x86, 0xDF, 0xBB, 0xDC, 0xDA, 0x77, 0xFF, 0xFC, 0x1A, 0xF8, 0xF3, 0xEE, 0xF5, 0xE9, 0xD9, 0x20, 0x14, 0xEE, 0xDB, 0x3D, 0xC7, 0xBB, 0xBF, 0x4B, 0xCF, 0x58, 0x6E, 0x4E, 0xFC, 0xEF, 0x3F, 0xCE, 0xAA, 0xDF, 0xBB, 0xDC, 0xDA, 0x72, 0xFF, 0xFD, 0x1A, 0xF8, 0xF3, 0xEE, 0xF5, 0xE8, 0xDE, 0xAC, 0xD4, 0x31, 0xC2, 0xD6, 0xC6, 0xBB, 0xBF, 0x98, 0x69, 0x81, 0xB9, 0x4F, 0xFB, 0xEF, 0x3F, 0x16, 0xBC, 0xE0, 0xBB, 0xD7, 0xDA, 0x6E, 0xFF, 0xFC, 0x1A, 0xF8, 0xF3, 0xEE, 0xF5, 0xE7, 0xE7, 0x72, 0xD4, 0xEE, 0xF1, 0xA7, 0xC6, 0xBB, 0xBF, 0xFC, 0x9C, 0x99, 0xC3, 0x7E, 0xFA, 0xEF, 0x3F, 0xCB, 0x0F, 0xE1, 0xBB, 0xD5, 0xDA, 0x6A, 0xFF, 0xFC, 0x1A, 0xF8, 0xF3, 0xEE, 0xF5, 0xE6, 0xE1, 0x09, 0xC9, 0x5A, 0xF7, 0x8B, 0xC6, 0xBB, 0xBF, 0x4B, 0x44, 0xA7, 0x19, 0x3D, 0xFA, 0xEF, 0x3F, 0x5E, 0x09, 0xE1, 0xBB, 0xD5, 0xDA, 0x66, 0xFF, 0xFC, 0x1A, 0xF8, 0xF3, 0xEE, 0xF5, 0xE5, 0xDF, 0xDE, 0xDC, 0xFB, 0x5F, 0x54, 0xC6, 0xBB, 0xBF, 0x2D, 0x9D, 0x98, 0x06, 0x41, 0xF9, 0xEF, 0x3F, 0x3F, 0xDE, 0xE0, 0xBB, 0xD6, 0xDA, 0x62, 0xFF, 0xFD, 0x1A, 0xF8, 0xF3, 0xEE, 0xF5, 0xE4, 0xDD, 0x46, 0x4C, 0xF1, 0xDA, 0xE6, 0xC5, 0xBB, 0xBF, 0xAF, 0x7A, 0x8D, 0xBC, 0x9F, 0xF7, 0xEF, 0x3F, 0xA6, 0x6D, 0xE1, 0xBB, 0xD3, 0xDA, 0x5E, 0xFF, 0xFC, 0x1A, 0xF8, 0xF3, 0xEE, 0xF5, 0xE3, 0xEA, 0x72, 0x0B, 0x3A, 0x60, }), 

new Tuple<Guid, byte[]>( Guid.Parse("26eb0024-b012-49a8-b1f8-394fb2032b0f"), new byte[] { 0x05, 0x3B, 0xC5, 0xBB, 0xBF, 0x33, 0x99, 0x61, 0xEF, 0x1D, 0xF5, 0xEF, 0x3F, 0xAA, 0x70, 0xE0, 0xBB, 0xD8, 0xDA, 0x5B, 0xFF, 0x4C, 0x69, }), 

new Tuple<Guid, byte[]>( Guid.Parse("26eb0024-b012-49a8-b1f8-394fb2032b0f"), new byte[] { 0x05, 0xFC, 0x1A, 0xF8, 0xF3, 0xEE, 0xF5, 0xE2, 0xE6, 0x8D, 0x15, 0x37, 0x50, 0x21, 0xC4, 0xBB, 0xBF, 0xFC, 0x2E, 0x99, 0xC0, 0xE1, 0xF0, 0xEF, 0x3F, 0xF7, 0xB7, 0xDE, 0xBB, 0xE0, 0xDA, 0x57, 0xFF, 0xFC, 0x1A, 0xF8, 0xF3, 0xEE, 0xF5, 0xE1, 0xDA, 0xA0, 0x90, 0xF3, 0x3B, 0x58, 0xC3, 0xBB, 0xBF, 0x2D, 0xDF, 0x7D, 0x91, 0xD0, 0xED, 0xEF, 0x3F, 0x88, 0x24, 0xDF, 0xBB, 0xDE, 0xDA, 0x48, 0xFF, 0xFC, 0x1A, 0xF8, 0xF3, 0xEE, 0xF5, 0xE0, 0xE9, 0xCC, 0x86, 0xD8, 0x81, 0xA1, 0xC2, 0xBB, 0xBF, 0xD7, 0xBE, 0x67, 0x15, 0x13, 0xE9, 0xEF, 0x3F, 0x6B, 0x34, 0xD9, 0xBB, 0xF9, 0xDA, 0x42, 0xFF, 0xFC, 0x1A, 0xF8, 0xF3, 0xEE, 0xF5, 0xDF, 0xEB, 0xF8, 0x79, 0x85, 0x02, 0x40, 0xC2, 0xBB, 0xBF, 0x60, 0x22, 0x3A, 0x0A, 0x32, 0xE7, 0xEF, 0x3F, 0xD2, 0xB0, 0xD7, 0xBB, 0x00, 0xDB, 0x3D, 0xFF, 0xFC, 0x1A, 0xF8, 0xF3, 0xEE, 0xF5, 0xDF, 0xE2, 0x05, 0xB6, 0xC3, 0x9B, 0x3E, 0xC2, 0xBB, 0xBF, 0x2E, 0xD4, 0x47, 0x37, 0x42, 0xE7, 0xEF, 0x3F, 0x1F, 0xD1, 0xD7, 0xBB, 0xFF, 0xDA, 0x3D, 0xFF, 0xFC, 0x1A, 0xF8, 0xF3, 0xEE, 0xF5, 0xDE, 0xE9, 0xA7, 0x9F, 0x6E, 0x63, 0x3E, 0xC2, 0xBB, 0xBF, 0xE2, 0x42, 0x46, 0xBB, 0xBF, 0xE7, 0xEF, 0x3F, 0x6D, 0xFD, 0xD7, 0xBB, 0xFE, 0xDA, 0x41, 0xFF, 0xFC, 0x1A, 0xF8, 0xF3, 0xEE, 0xF5, 0xDE, 0xDE, 0x8C, 0x1F, 0x13, 0xBE, 0x40, 0xC2, 0xBB, 0xBF, 0xD5, 0x9B, 0x71, 0xBB, 0xD6, 0xE7, 0xEF, 0x3F, 0x22, 0x00, 0xD8, 0xBB, 0xFE, 0xDA, 0x43, 0xFF, 0xFC, 0x1A, 0xF8, 0xF3, 0xEE, 0xF5, 0xDD, 0xE6, 0x78, 0x51, 0x50, 0xEE, }), 

new Tuple<Guid, byte[]>( Guid.Parse("26eb0024-b012-49a8-b1f8-394fb2032b0f"), new byte[] { 0x05, 0x4B, 0xC2, 0xBB, 0xBF, 0x96, 0x5A, 0xC5, 0x86, 0x28, 0xE8, 0xEF, 0x3F, 0x49, 0x16, 0xD8, 0xBB, 0xFE, 0xDA, 0x4B, 0xFF, 0x98, 0x0E, }), 

new Tuple<Guid, byte[]>( Guid.Parse("26eb0024-b012-49a8-b1f8-394fb2032b0f"), new byte[] { 0x05, 0xFC, 0x1A, 0xF8, 0xF3, 0xEE, 0xF5, 0xDC, 0xFA, 0x60, 0x3C, 0x31, 0xC2, 0x54, 0xC2, 0xBB, 0xBF, 0xD8, 0xEF, 0x16, 0x0E, 0x3A, 0xE8, 0xEF, 0x3F, 0x27, 0x24, 0xD8, 0xBB, 0xFE, 0xDA, 0x50, 0xFF, 0xFC, 0x1A, 0xF8, 0xF3, 0xEE, 0xF5, 0xDC, 0xEF, 0x84, 0x9E, 0x21, 0x2C, 0x58, 0xC2, 0xBB, 0xBF, 0xFA, 0x97, 0x46, 0xE7, 0x45, 0xE8, 0xEF, 0x3F, 0x52, 0x3B, 0xD8, 0xBB, 0xFD, 0xDA, 0x51, 0xFF, 0xFC, 0x1A, 0xF8, 0xF3, 0xEE, 0xF5, 0xDC, 0xCA, 0xA7, 0xA5, 0xCF, 0x50, 0x61, 0xC2, 0xBB, 0xBF, 0x75, 0xD0, 0x81, 0x64, 0x40, 0xE8, 0xEF, 0x3F, 0x2E, 0x3D, 0xD8, 0xBB, 0xFD, 0xDA, 0x55, 0xFF, 0xFC, 0x1A, 0xF8, 0xF3, 0xEE, 0xF5, 0xDB, 0xEF, 0x42, 0x7B, 0xDD, 0x63, 0x65, 0xC2, 0xBB, 0xBF, 0x6B, 0x2E, 0x47, 0xEA, 0x5C, 0xE8, 0xEF, 0x3F, 0x2E, 0x3D, 0xD8, 0xBB, 0xFD, 0xDA, 0x57, 0xFF, 0xFC, 0x1A, 0xF8, 0xF3, 0xEE, 0xF5, 0xDA, 0xEC, 0xE4, 0x57, 0xCB, 0x2F, 0x6C, 0xC2, 0xBB, 0xBF, 0x28, 0xD7, 0x51, 0x21, 0x9D, 0xE8, 0xEF, 0x3F, 0xC7, 0x94, 0xD8, 0xBB, 0xFC, 0xDA, 0x5C, 0xFF, 0xFC, 0x1A, 0xF8, 0xF3, 0xEE, 0xF5, 0xD9, 0xFB, 0xC0, 0xA0, 0xC2, 0xDC, 0x71, 0xC2, 0xBB, 0xBF, 0xB4, 0xEE, 0x55, 0x7A, 0x1E, 0xE9, 0xEF, 0x3F, 0xCB, 0xAC, 0xD8, 0xBB, 0xFB, 0xDA, 0x5F, 0xFF, 0xFC, 0x1A, 0xF8, 0xF3, 0xEE, 0xF5, 0xD9, 0xEF, 0x0B, 0xC1, 0xF9, 0x3A, 0x70, 0xC2, 0xBB, 0xBF, 0x87, 0x9B, 0x9B, 0x6D, 0x35, 0xE9, 0xEF, 0x3F, 0x83, 0xC7, 0xD8, 0xBB, 0xFB, 0xDA, 0x60, 0xFF, 0xFC, 0x1A, 0xF8, 0xF3, 0xEE, 0xF5, 0xD8, 0xEB, 0xEB, 0x37, 0xD0, 0x45, }), 

new Tuple<Guid, byte[]>( Guid.Parse("26eb0024-b012-49a8-b1f8-394fb2032b0f"), new byte[] { 0x05, 0x6F, 0xC2, 0xBB, 0xBF, 0x86, 0x74, 0x09, 0x24, 0x12, 0xE9, 0xEF, 0x3F, 0xC6, 0x88, 0xD8, 0xBB, 0xFC, 0xDA, 0x63, 0xFF, 0xC7, 0xF3, }), 




        };

        public PreviousDataTransmitReplayer3(IObserver<Tuple<Guid, byte[]>> observer)
        {
            this.observer = observer;
        }

        public void Execute()
        {
            foreach(var data in previousData)
            {
                observer.OnNext(data);
            }
        }
    }
}