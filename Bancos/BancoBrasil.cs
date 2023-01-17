﻿using Newtonsoft.Json;
using PixNET.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace PixNET.Services.Pix.Bancos
{
    public class BancoBrasil : Banco
    {
        public BancoBrasil(PixAmbiente? ambiente)
        {
            endpoint = new Endpoint
            {
                AuthorizationToken = ambiente == PixAmbiente.Producao ? "https://oauth.bb.com.br/oauth/token" : "https://oauth.sandbox.bb.com.br/oauth/token",
                Pix = ambiente == PixAmbiente.Producao ? "https://api.bb.com.br/pix/v1/" : "https://api.sandbox.bb.com.br/pix/v1/"
            };
        }

        public override async Task GetAccessTokenAsync(bool force = false)
        {

            string parameters = "grant_type=client_credentials&scope=cob.read cob.write pix.read pix.write";
            List<string> headers = new List<string>();

            headers.Add(string.Format("Authorization: Basic {0}", Utils.Base64Encode(_credentials.clientId + ":" + _credentials.clientSecret)));
            headers.Add($"X-Developer-Application-Key: {_credentials.developerKey}");
            string
                request = await Utils.sendRequestAsync(endpoint.AuthorizationToken, parameters, "POST", headers, 0, "application/x-www-form-urlencoded", true, _certificate);

            try
            {
                token = JsonConvert.DeserializeObject<AccessToken>(request);
                token.lastTokenTime = DateTime.Now;
            }
            catch { }

        }

        public override void GetAccessToken(bool force = false)
        {
            string parameters = "grant_type=client_credentials&scope=cob.read cob.write pix.read pix.write";
            List<string> headers = new List<string>();

            headers.Add($"X-Developer-Application-Key: {_credentials.developerKey}");
            headers.Add(string.Format("Authorization: Basic {0}", Utils.Base64Encode(_credentials.clientId + ":" + _credentials.clientSecret)));
            parameters += $"client_id={_credentials.clientId}&client_secret={_credentials.clientSecret}";


            string
                request = Utils.sendRequest(endpoint.AuthorizationToken, parameters, "POST", headers, 0, "application/x-www-form-urlencoded", true, _certificate);

            try
            {
                token = JsonConvert.DeserializeObject<AccessToken>(request);
                token.lastTokenTime = DateTime.Now;
            }
            catch { }
        }

        public override async Task<PixPayload> CreateCobrancaAsync()
        {
            await GetAccessTokenAsync();
            if (token != null)
            {
                string parameters = JsonConvert.SerializeObject(_payload, Formatting.None, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });
                List<string> headers = new List<string>();

                headers.Add($"X-Developer-Application-Key: {_credentials.developerKey}");
                headers.Add(string.Format("Authorization: Bearer {0}", token.access_token));
                string request = null;
                try
                {
                    request = await Utils.sendRequestAsync(endpoint.Pix + "cob/" + ((PixPayload)_payload).txid, parameters, "PUT", headers, 0, "application/json", true, _certificate);
                }
                catch(Exception ex)
                {
                    Model.Errors.BancoBrasil.Errors error = JsonConvert.DeserializeObject<Model.Errors.BancoBrasil.Errors>(ex.Message);
                    string errors = String.Join(Environment.NewLine, error.erros.Select(T => T.mensagem).ToArray());
                    throw new Exception(errors);
                }
                PixPayload cobranca = null;

                try
                {
                    cobranca = JsonConvert.DeserializeObject<PixPayload>(request);
                    cobranca.textoImagemQRcode = GerarQrCode(cobranca);
                }
                catch { }

                token = null;
                request = null;
                return cobranca;

            }
            return null;
        }
        public override PixPayload CreateCobranca()
        {
            GetAccessToken();
            if (token != null)
            {
                string parameters = JsonConvert.SerializeObject(_payload, Formatting.None, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });
                List<string> headers = new List<string>();

                headers.Add($"X-Developer-Application-Key: {_credentials.developerKey}");
                headers.Add(string.Format("Authorization: Bearer {0}", token.access_token));
                string request = null;
                try
                {
                    request = Utils.sendRequest(endpoint.Pix + "cob/" + ((PixPayload)_payload).txid, parameters, "PUT", headers, 0, "application/json", true, _certificate);
                }
                catch (Exception ex)
                {
                    Model.Errors.BancoBrasil.Errors error = JsonConvert.DeserializeObject<Model.Errors.BancoBrasil.Errors>(ex.Message);
                    string errors = String.Join(Environment.NewLine, error.erros.Select(T => T.mensagem).ToArray());
                    throw new Exception(errors);
                }

                PixPayload cobranca = null;

                try
                {
                    cobranca = JsonConvert.DeserializeObject<PixPayload>(request);
                    cobranca.textoImagemQRcode = GerarQrCode(cobranca);
                }
                catch { }

                token = null;
                request = null;
                return cobranca;

            }
            return null;
        }
        public override async Task<PixPayload> ConsultaCobrancaAsync(string txid)
        {
            await GetAccessTokenAsync();
            if (token != null)
            {
                List<string> headers = new List<string>();

                headers.Add($"X-Developer-Application-Key: {_credentials.developerKey}");
                headers.Add(string.Format("Authorization: Bearer {0}", token.access_token));
                string request = await Utils.sendRequestAsync(endpoint.Pix + "cob/" + txid, "", "GET", headers, 0, "", false, _certificate);
                PixPayload cobranca = null;
                try
                {
                    cobranca = JsonConvert.DeserializeObject<PixPayload>(request);
                    cobranca.textoImagemQRcode = GerarQrCode(cobranca);
                }
                catch { }
                return cobranca;

            }
            return null;
        }
        public override async Task<PixPayload> CancelarPixAsync()
        {
            await GetAccessTokenAsync();
            if (token != null)
            {
                ((PixPayload)_payload).status = "REMOVIDA_PELO_USUARIO_RECEBEDOR";
                string parameters = JsonConvert.SerializeObject(_payload, Formatting.None, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });
                List<string> headers = new List<string>();

                headers.Add($"X-Developer-Application-Key: {_credentials.developerKey}");
                headers.Add(string.Format("Authorization: Bearer {0}", token.access_token));
                string request = null;
                try
                {
                    request = await Utils.sendRequestAsync(endpoint.Pix + "cob/" + ((PixPayload)_payload).txid, parameters, "PATCH", headers, 0, "application/json", true, _certificate);
                }
                catch (Exception ex)
                {
                    Model.Errors.BancoBrasil.Errors error = JsonConvert.DeserializeObject<Model.Errors.BancoBrasil.Errors>(ex.Message);
                    string errors = String.Join(Environment.NewLine, error.erros.Select(T => T.mensagem).ToArray());
                    throw new Exception(errors);
                }
                PixPayload cobranca = null;

                try
                {
                    cobranca = JsonConvert.DeserializeObject<PixPayload>(request);
                    cobranca.textoImagemQRcode = GerarQrCode(cobranca);
                }
                catch { }

                token = null;
                request = null;
                return cobranca;

            }
            return null;
        }
        public override async Task<List<Model.Pix>> ConsultaPixRecebidosAsync()
        {
            List<Model.Pix> listaPix = new List<Model.Pix>();

            try
            {
                await GetAccessTokenAsync();
                if (token != null)
                {
                    string parameters = JsonConvert.SerializeObject(_payload, Formatting.None, new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore
                    });

                    List<string> headers = new List<string>();

                    headers.Add($"X-Developer-Application-Key: {_credentials.developerKey}");
                    headers.Add(string.Format("Authorization: Bearer {0}", token.access_token));
                    int
                        paginaAtual = 0;

                    string request = null;

                    bool loop = true;

                    PixRecebidos cobranca = null;
                    while (loop)
                    {
                        string queryString = string.Format
                            (
                                "txIdPresente=true&inicio={0}&fim={1}&paginaAtual={2}&paginacao.paginaAtual={2}",
                                ((PixRecebidosPayload)_payload).inicio.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                                ((PixRecebidosPayload)_payload).fim.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                                paginaAtual
                            );
                        request = await Utils.sendRequestAsync(endpoint.Pix + "pix?" + queryString, null, "GET", headers, 0, "application/json", false, _certificate);
                        try
                        {
                            cobranca = JsonConvert.DeserializeObject<PixRecebidos>(request);
                            if (hasTxId)
                                listaPix.AddRange(cobranca.pix.Where(T => !String.IsNullOrEmpty(T.txid)));
                            else
                                listaPix.AddRange(cobranca.pix);

                            if (
                                cobranca.parametros.paginacao.paginaAtual + 1 < cobranca.parametros.paginacao.quantidadeDePaginas &&
                                cobranca.parametros.paginacao.quantidadeTotalDeItens > listaPix.Count)
                            {
                                paginaAtual = cobranca.parametros.paginacao.paginaAtual + 1;
                                continue;
                            }
                            else
                            {
                                loop = false;
                                break;
                            }
                        }
                        catch
                        {
                            loop = false;
                            break;
                        }
                    }

                    return listaPix;

                }
                return null;
            }
            catch (WebException ex)
            {
                try
                {
                    PixError er = JsonConvert.DeserializeObject<PixError>(ex.Message);
                    if (er != null)
                    {
                        if (er.erros.Find(T => T.codigo == "4769515") != null)
                            return listaPix;
                    }
                }
                catch { }
                throw ex;
            }
        }
    }
}
