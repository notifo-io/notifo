import { BaseAPI, Configuration } from "../generated";
import { NotifoOptions, TokenStore } from "./NotifoClient";
import { NotifoUnauthorizedError } from "./errors";

export class TokenAPI extends BaseAPI {
    private tokenPromise?: Promise<{ type: string; value: string }>;

    constructor(
        private readonly tokenStore: TokenStore,
        private readonly clientOptions: NotifoOptions,
        configuration: Configuration,
    ) {
        super(configuration);
    }

    public async getToken() {
        const promise = (this.tokenPromise ||= (async () => {
            const { apiKey, clientId, clientSecret } = this.clientOptions;
            if (apiKey) {
                return { type: "ApiKey", value: apiKey };
            }

            if (!clientId || !clientSecret) {
                throw new NotifoUnauthorizedError(undefined, undefined, "Client ID and secret not defined.");
            }

            const now = new Date().getTime();
            try {
                let token = this.tokenStore.get();
                if (token != null && token.expiresAt > now) {
                    return { type: "Bearer", value: token.accessToken };
                }

                const response = await this.request({
                    path: "/connect/token",
                    headers: {
                        "Content-Type": "application/x-www-form-urlencoded",
                        "X-AuthRequest": "1",
                        "X-AuthSource": "Client",
                    },
                    body: new URLSearchParams({
                        grant_type: "client_credentials",
                        client_id: clientId,
                        client_secret: clientSecret,
                        scope: "NotifoAPI",
                    }),
                    method: "POST",
                });

                const { access_token: accessToken, expires_in: expiresIn } = (await response.json()) as {
                    access_token: string;
                    expires_in: number;
                };

                if (typeof accessToken !== "string") {
                    throw new NotifoUnauthorizedError(undefined, undefined, "Token is not a string");
                }

                if (typeof expiresIn !== "number") {
                    throw new NotifoUnauthorizedError(undefined, undefined, "Token has no valid expiration");
                }

                token = {
                    accessToken,
                    expiresIn,
                    expiresAt: now + expiresIn,
                };

                this.tokenStore.set(token);
                return { type: "Bearer", value: token.accessToken };
            } finally {
                this.tokenPromise = undefined;
            }
        })());

        return promise;
    }
}
