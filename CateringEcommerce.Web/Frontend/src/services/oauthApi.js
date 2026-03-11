import { fetchApi } from './apiUtils';

// ===================================
// INITIATE OAUTH LOGIN
// ===================================
export const initiateOAuthLogin = async (provider) => {
  try {
    const response = await fetchApi(`/OAuth/${provider}/Login`, 'GET');
    return response;
  } catch (error) {
    console.error(`Error initiating ${provider} login:`, error);
    throw error;
  }
};

// ===================================
// HANDLE OAUTH CALLBACK
// ===================================
export const handleOAuthCallback = async (provider, code, state) => {
  try {
    const response = await fetchApi(
      `/OAuth/${provider}/Callback?code=${encodeURIComponent(code)}&state=${encodeURIComponent(state)}`,
      'GET'
    );
    return response;
  } catch (error) {
    console.error(`Error handling ${provider} callback:`, error);
    throw error;
  }
};

// ===================================
// GET CONNECTED ACCOUNTS
// ===================================
export const getConnectedAccounts = async () => {
  try {
    const response = await fetchApi('/OAuth/Connected-Accounts', 'GET');
    return response;
  } catch (error) {
    console.error('Error fetching connected accounts:', error);
    throw error;
  }
};

// ===================================
// LINK OAUTH ACCOUNT
// ===================================
export const linkOAuthAccount = async (provider, code, state) => {
  try {
    const response = await fetchApi('/OAuth/Link-Account', 'POST', {
      provider,
      code,
      state
    });
    return response;
  } catch (error) {
    console.error('Error linking OAuth account:', error);
    throw error;
  }
};

// ===================================
// UNLINK OAUTH ACCOUNT
// ===================================
export const unlinkOAuthAccount = async (oauthId) => {
  try {
    const response = await fetchApi(`/OAuth/Unlink-Account/${oauthId}`, 'DELETE');
    return response;
  } catch (error) {
    console.error('Error unlinking OAuth account:', error);
    throw error;
  }
};

// ===================================
// SET PRIMARY OAUTH CONNECTION
// ===================================
export const setPrimaryConnection = async (oauthId) => {
  try {
    const response = await fetchApi(`/OAuth/Set-Primary/${oauthId}`, 'PUT');
    return response;
  } catch (error) {
    console.error('Error setting primary connection:', error);
    throw error;
  }
};

// ===================================
// GET ACTIVE PROVIDERS
// ===================================
export const getActiveProviders = async () => {
  try {
    const response = await fetchApi('/OAuth/Providers', 'GET');
    return response;
  } catch (error) {
    console.error('Error fetching active providers:', error);
    throw error;
  }
};
