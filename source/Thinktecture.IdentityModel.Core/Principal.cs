﻿/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see LICENSE
 */

using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Thinktecture.IdentityModel
{
    public static class Principal
    {
        public static ClaimsPrincipal Anonymous
        {
            get
            {
                var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, "")
                    };

                var anonId = new ClaimsIdentity(claims);
                var anonPrincipal = new ClaimsPrincipal(anonId);

                return anonPrincipal;
            }
        }

        public static ClaimsPrincipal Create(string authenticationType, params Claim[] claims)
        {
            return new ClaimsPrincipal(new ClaimsIdentity(claims, authenticationType));
        }

        public static IEnumerable<Claim> CreateRoles(string[] roleNames)
        {
            if (roleNames == null || roleNames.Count() == 0)
            {
                return new Claim[] { };
            }

            return new List<Claim>(from r in roleNames select new Claim(ClaimTypes.Role, r)).ToArray();
        }

        public static ClaimsPrincipal CreateFromCertificate(X509Certificate2 certificate, bool includeAllClaims = false)
        {
            var claims = new List<Claim>();
            var issuer = certificate.Issuer;

            claims.Add(new Claim("issuer", issuer));

            var thumbprint = certificate.Thumbprint;
            claims.Add(new Claim(ClaimTypes.Thumbprint, thumbprint, ClaimValueTypes.Base64Binary, issuer));

            string name = certificate.SubjectName.Name;
            if (!string.IsNullOrEmpty(name))
            {
                claims.Add(new Claim(ClaimTypes.X500DistinguishedName, name, ClaimValueTypes.String, issuer));
            }

            if (includeAllClaims)
            {
                name = certificate.SerialNumber;
                if (!string.IsNullOrEmpty(name))
                {
                    claims.Add(new Claim(ClaimTypes.SerialNumber, name, "http://www.w3.org/2001/XMLSchema#string", issuer));
                }

                name = certificate.GetNameInfo(X509NameType.DnsName, false);
                if (!string.IsNullOrEmpty(name))
                {
                    claims.Add(new Claim(ClaimTypes.Dns, name, ClaimValueTypes.String, issuer));
                }

                name = certificate.GetNameInfo(X509NameType.SimpleName, false);
                if (!string.IsNullOrEmpty(name))
                {
                    claims.Add(new Claim(ClaimTypes.Name, name, ClaimValueTypes.String, issuer));
                }

                name = certificate.GetNameInfo(X509NameType.EmailName, false);
                if (!string.IsNullOrEmpty(name))
                {
                    claims.Add(new Claim(ClaimTypes.Email, name, ClaimValueTypes.String, issuer));
                }

                name = certificate.GetNameInfo(X509NameType.UpnName, false);
                if (!string.IsNullOrEmpty(name))
                {
                    claims.Add(new Claim(ClaimTypes.Upn, name, ClaimValueTypes.String, issuer));
                }

                name = certificate.GetNameInfo(X509NameType.UrlName, false);
                if (!string.IsNullOrEmpty(name))
                {
                    claims.Add(new Claim(ClaimTypes.Uri, name, ClaimValueTypes.String, issuer));
                }

                RSA key = certificate.PublicKey.Key as RSA;
                if (key != null)
                {
                    claims.Add(new Claim(ClaimTypes.Rsa, key.ToXmlString(false), ClaimValueTypes.RsaKeyValue, issuer));
                }

                DSA dsa = certificate.PublicKey.Key as DSA;
                if (dsa != null)
                {
                    claims.Add(new Claim(ClaimTypes.Dsa, dsa.ToXmlString(false), ClaimValueTypes.DsaKeyValue, issuer));
                }

                var expiration = certificate.GetExpirationDateString();
                if (!string.IsNullOrEmpty(expiration))
                {
                    claims.Add(new Claim(ClaimTypes.Expiration, expiration, ClaimValueTypes.DateTime, issuer));
                }
            }

            var id = new ClaimsIdentity(claims, "X.509");
            return new ClaimsPrincipal(id);
        }
    }
}
