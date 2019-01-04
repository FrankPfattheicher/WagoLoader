﻿//Copyright (c) 2013 Josip Medved <jmedved@jmedved.com>
//2013-03-26: Initial version.

using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
// ReSharper disable InconsistentNaming
// ReSharper disable BuiltInTypeReferenceStyle
// ReSharper disable UnusedMember.Global

namespace WagoLoader.Loader
{
    public static class Password
    {
        private const Int32 MinimumSaltSize = 0;
        /// <summary>
        {
        /// <summary>
        {
            {
            else
            {
        /// <summary>
        {
        /// <summary>
        {
            if (saltSize < MinimumSaltSize) { saltSize = MinimumSaltSize; }
            var salt = new byte[saltSize];
            for (var i = 0; i < salt.Length; i++)
            { //make it an ascii
            return Create(Utf8WithoutBom.GetBytes(password), salt, algorithm, iterationCount);
        /// <summary>
        {
            if (iterationCount != 0)
            { //silently setup iterationCount to allowable limits (except for default value)
            if (algorithm == PasswordAlgorithm.Default) { algorithm = PasswordAlgorithm.Sha512; }
            {
        /// <summary>
        {
        /// <summary>
        {
            string hashCalc;
            switch (id)
            { //algorithm
            return string.Equals(hash, hashCalc);

        private static string CreateSha256(byte[] password, byte[] salt, int iterationCount)
        {
            var sb = new StringBuilder();
            {
            sb.Append(Encoding.ASCII.GetString(salt));
        private static string CreateSha512(byte[] password, byte[] salt, int iterationCount)
        {
            var sb = new StringBuilder();
            {
            sb.Append(Encoding.ASCII.GetString(salt));
        private static byte[] CreateSha(string hashName, byte[] password, byte[] salt, int iterationCount)
        {
            { //step 1
                byte[] hashB;
                { //step 4
                AddRepeatedDigest(digestA, hashB, password.Length); //step 9/10
                var passwordLength = password.Length;
                { //step 11
                    { //bit 1
                    else
                    { //bit 0
                    passwordLength >>= 1;
                hashA = FinishDigest(digestA); //step 12
            byte[] hashDP;
            { //step 13
                { //step 14
                hashDP = FinishDigest(digestDP); //step 15
            var p = ProduceBytes(hashDP, password.Length); //step 16
            byte[] hashDS;
            { //step 17
                { //step 18
                hashDS = FinishDigest(digestDS); //step 19
            var s = ProduceBytes(hashDS, salt.Length); //step 20
            var hashAC = hashA;
            { //step 21
                { //step 21a
                    { //step 21b
                    else
                    { //step 21c
                    if ((i % 3) != 0) { AddDigest(digestC, s); } //step 21d
                    if ((i % 7) != 0) { AddDigest(digestC, p); } //step 21e
                    { //step 21f
                    else
                    { //step 21g
                    hashAC = FinishDigest(digestC); //step 21h
            }
            return hashAC;
        #endregion
        #region MD5
        private static string CreateMd5Basic(byte[] password, byte[] salt, int iterationCount)
        {
            return CreateMd5(password, salt, iterationCount, "$1$");
        private static string CreateMd5Apache(byte[] password, byte[] salt, int iterationCount)
        {
            return CreateMd5(password, salt, iterationCount, "$apr1$");
        private static string CreateMd5(byte[] password, byte[] salt, int iterationCount, string magic)
        {
            { //step 1
                byte[] hashB;
                { //step 4
                AddRepeatedDigest(digestA, hashB, password.Length); //step 9/10
                var passwordLength = password.Length;
                { //step 11
                    { //bit 1
                    else
                    { //bit 0
                hashA = FinishDigest(digestA); //step 12
            var hashAC = hashA;
            { //step 21
                { //step 21a
                    { //step 21b
                    else
                    { //step 21c
                    if ((i % 3) != 0) { AddDigest(digestC, salt); } //step 21d
                    if ((i % 7) != 0) { AddDigest(digestC, password); } //step 21e
                    { //step 21f
                    else
                    { //step 21g
                    hashAC = FinishDigest(digestC); //step 21h
            var c = hashAC;
            if (iterationCount != Md5DefaultIterationCount)
            {
            sb.Append(Encoding.ASCII.GetString(salt));
        #endregion
        #region Helpers
        private static void AddDigest(HashAlgorithm digest, byte[] bytes)
        {
            var offset = 0;
            {
        private static void AddRepeatedDigest(HashAlgorithm digest, byte[] bytes, int length)
        {
            {
        private static byte[] ProduceBytes(byte[] hash, int length)
        {
            while (length > 0)
            {
            return produced;
        private static byte[] FinishDigest(HashAlgorithm digest)
        {
        private static void Base64TripletFill(StringBuilder sb, byte? byte2, byte? byte1, byte? byte0)
        {
        private static bool SplitHashedPassword(string hashedPassword, out string hash)
        {
        private static bool SplitHashedPassword(string hashedPassword, out string id, out int iterationCount, out byte[] salt, out string hash)
        {
            var parts = hashedPassword.Split('$');
            id = parts[1];
            {
            else
            {
            return true;
        #endregion
    }
    /// <summary>
    public enum PasswordAlgorithm
    {
        /// <summary>
        /// <summary>
        /// <summary>
        /// <summary>
}