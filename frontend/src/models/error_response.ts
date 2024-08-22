export default interface ErrorResponse {
    error: string;
    error_description?: string;
    error_uri?: URL;
    state?: string;
}
