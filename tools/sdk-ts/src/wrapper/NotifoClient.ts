import {
    AppsApi,
    AppsApiInterface,
    ConfigsApi,
    ConfigsApiInterface,
    Configuration,
    ConfigurationParameters,
    DiagnosticsApi,
    DiagnosticsApiInterface,
    EmailTemplatesApi,
    EmailTemplatesApiInterface,
    EventsApi,
    EventsApiInterface,
    FetchAPI,
    LogsApi,
    LogsApiInterface,
    MediaApi,
    MediaApiInterface,
    MessagingTemplatesApi,
    MessagingTemplatesApiInterface,
    Middleware,
    MobilePushApi,
    MobilePushApiInterface,
    NotificationsApi,
    NotificationsApiInterface,
    PingApi,
    PingApiInterface,
    ResponseError,
    SmsTemplatesApi,
    SmsTemplatesApiInterface,
    SystemUsersApi,
    SystemUsersApiInterface,
    TemplatesApi,
    TemplatesApiInterface,
    TopicsApi,
    TopicsApiInterface,
    UserApi,
    UserApiInterface,
    UsersApi,
    UsersApiInterface,
} from "../generated";
import { buildError } from "./errors";
import { addHeader, getHeader } from "./headers";
import { TokenAPI } from "./TokenAPI";

export interface NotifoOptions {
    /**
     * The secret of the client.
     */
    clientId?: string;

    /**
     * The ID of the client.
     */
    clientSecret?: string;

    /**
     * The API key.
     */
    apiKey?: string;

    /**
     * Custom headers to be added to each request.
     */
    customHeaders?: Record<string, string>;

    /**
     * The URL to your Notifo installation (cloud by default).
     */
    url?: string;

    /**
     * A custom fetcher for normal requests.
     */
    fetcher?: FetchAPI;

    /**
     * A function to create a new fetcher based on the default fetcher.
     */
    middleware?: Middleware;

    /**
     * The timeout in milliseconds.
     */
    timeout?: number;

    /**
     * The store for tokens. By default it is in memory.
     */
    tokenStore?: TokenStore;
}

export interface TokenStore {
    get(): Token | undefined;

    set(token: Token): void;

    clear(): void;
}

export interface Token {
    accessToken: string;
    expiresIn: number;
    expiresAt: number;
}

export class NotifoClients {
    private readonly configuration: Configuration;
    private readonly tokenStore: TokenStore;
    private readonly tokenApi: TokenAPI;
    private appsApi?: AppsApi;
    private configsApi?: ConfigsApi;
    private diagnosticsApi?: DiagnosticsApi;
    private emailTemplatesApi?: EmailTemplatesApi;
    private eventsApi?: EventsApi;
    private logsApi?: LogsApi;
    private mediaApi?: MediaApi;
    private messagingTemplatesApi?: MessagingTemplatesApi;
    private mobilePushApi?: MobilePushApi;
    private notificationsApi?: NotificationsApi;
    private pingApi?: PingApi;
    private smsTemplatesApi?: SmsTemplatesApi;
    private systemUsersApi?: SystemUsersApi;
    private templatesApi?: TemplatesApi;
    private topisApi?: TopicsApi;
    private userApi?: UserApi;
    private usersApi?: UsersApi;

    public get apps(): AppsApiInterface {
        return (this.appsApi ??= new AppsApi(this.configuration));
    }

    public get configs(): ConfigsApiInterface {
        return (this.configsApi ??= new ConfigsApi(this.configuration));
    }

    public get diagnostics(): DiagnosticsApiInterface {
        return (this.diagnosticsApi ??= new DiagnosticsApi(this.configuration));
    }

    public get emailTemplates(): EmailTemplatesApiInterface {
        return (this.emailTemplatesApi ??= new EmailTemplatesApi(this.configuration));
    }

    public get events(): EventsApiInterface {
        return (this.eventsApi ??= new EventsApi(this.configuration));
    }

    public get logs(): LogsApiInterface {
        return (this.logsApi ??= new LogsApi(this.configuration));
    }

    public get media(): MediaApiInterface {
        return (this.mediaApi ??= new MediaApi(this.configuration));
    }

    public get messagingTemplates(): MessagingTemplatesApiInterface {
        return (this.messagingTemplatesApi ??= new MessagingTemplatesApi(this.configuration));
    }

    public get mobilePush(): MobilePushApiInterface {
        return (this.mobilePushApi ??= new MobilePushApi(this.configuration));
    }

    public get notifications(): NotificationsApiInterface {
        return (this.notificationsApi ??= new NotificationsApi(this.configuration));
    }

    public get ping(): PingApiInterface {
        return (this.pingApi ??= new PingApi(this.configuration));
    }

    public get smsTemplates(): SmsTemplatesApiInterface {
        return (this.smsTemplatesApi ??= new SmsTemplatesApi(this.configuration));
    }

    public get systemUsers(): SystemUsersApiInterface {
        return (this.systemUsersApi ??= new SystemUsersApi(this.configuration));
    }

    public get templates(): TemplatesApiInterface {
        return (this.templatesApi ??= new TemplatesApi(this.configuration));
    }

    public get topics(): TopicsApiInterface {
        return (this.topisApi ??= new TopicsApi(this.configuration));
    }

    public get user(): UserApiInterface {
        return (this.userApi ??= new UserApi(this.configuration));
    }

    public get users(): UsersApiInterface {
        return (this.usersApi ??= new UsersApi(this.configuration));
    }

    /**
     * The current API key.
     */
    public get apiKey() {
        return this.clientOptions.apiKey;
    }

    /**
     * The current client ID.
     */
    public get clientId() {
        return this.clientOptions.clientId;
    }

    /**
     * The current client secret.
     */
    public get clientSecret() {
        return this.clientOptions.clientSecret;
    }

    /**
     * The current URL to the Notifo installation.
     */
    public get url() {
        return this.clientOptions.url!;
    }

    constructor(readonly clientOptions: NotifoOptions) {
        clientOptions.url ||= "https://app.notifo.io";

        Object.freeze(clientOptions);

        this.tokenStore = this.clientOptions.tokenStore || new InMemoryTokenStore();

        const fetchCore = this.clientOptions.fetcher || fetch;
        const fetchApi: FetchAPI = async (input, init) => {
            init ||= {};

            addOptions(init, clientOptions);

            if (!getHeader(init, "X-AuthRequest")) {
                addHeader(init, "Authorization", `Bearer ${await this.tokenApi.getToken()}`);
            }

            let response: Response;
            try {
                response = await fetchCore(input, init);

                if (response && response.status === 401 && !getHeader(init, "X-Retry")) {
                    addHeader(init, "X-Retry", "1");
                    this.clearToken();
                    return await fetchApi(input, init);
                }
            } catch (error: unknown) {
                throw await buildError(error);
            }

            if (response && response.status >= 200 && response.status < 300) {
                return response;
            }

            const cause = new ResponseError(response, "Response returned an error code");
            throw await buildError(cause);
        };

        const parameters: ConfigurationParameters = {
            basePath: clientOptions.url || "https://app.notifo.io",
            fetchApi,
        };

        if (clientOptions.middleware) {
            parameters.middleware = [clientOptions.middleware];
        }

        this.configuration = new Configuration(parameters);

        // Create an empty API object for the token.
        this.tokenApi = new TokenAPI(this.tokenStore, clientOptions, this.configuration);
    }

    /**
     * Clears the current token in case it has been expired.
     */
    clearToken() {
        this.tokenStore.clear();
    }

    /**
     * Get an access token from the token store. If it doesn't exist, request it and save it to the token store
     */
    getToken() {
        return this.tokenApi.getToken();
    }
}

function addOptions(init: RequestInit, clientOptions: NotifoOptions) {
    if (clientOptions.timeout) {
        init.signal = AbortSignal.timeout(clientOptions.timeout);
    }

    if (clientOptions.customHeaders) {
        for (const [key, value] of Object.entries(clientOptions.customHeaders)) {
            addHeader(init, key, value);
        }
    }
}

export class InMemoryTokenStore implements TokenStore {
    private token: Token | undefined;

    get(): Token | undefined {
        return this.token;
    }

    set(token: Token): void {
        this.token = token;
    }

    clear() {
        this.token = undefined;
    }
}

export class StorageTokenStore implements TokenStore {
    constructor(
        readonly store: Storage = localStorage,
        readonly key = "NotifoToken",
    ) {}

    get(): Token | undefined {
        const value = this.store.getItem(this.key);

        if (!value) {
            return undefined;
        }

        return JSON.parse(value);
    }

    set(token: Token): void {
        this.store.setItem(this.key, JSON.stringify(token));
    }

    clear() {
        this.store.removeItem(this.key);
    }
}
