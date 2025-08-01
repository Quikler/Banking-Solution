(function () {
  console.log("[custom-auth.js]: Running");

  const tokenKey = "jwt_token"; // Key to store the token in localStorage
  const refreshUrl = "/api/v1/accounts/refresh"; // Refresh token endpoint

  console.log("Token: " + localStorage.getItem(tokenKey));

  // Intercept API responses
  const originalFetch = window.fetch;
  window.fetch = async (url, options) => {
    let response = await originalFetch(url, options);

    if (url.includes("/api/v1/accounts/login") || url.includes("/api/v1/accounts/signup")) {
      const data = await response.clone().json();
      if (data.token) {
        localStorage.setItem(tokenKey, data.token);
        setSwaggerAuth(data.token);
      }
    }

    // If response is 401 and not the refresh token endpoint, try refreshing
    if (response.status === 401 && !url.includes(refreshUrl)) {
      console.log("[custom-auth.js]: Token expired, attempting refresh...");
      const newToken = await refreshAccessToken();
      if (newToken) {
        console.log("[custom-auth.js]: Token refreshed, retrying request...");
        setSwaggerAuth(newToken);
        return retryRequest(url, options, newToken);
      } else {
        console.log("[custom-auth.js]: Token refresh failed. Logging out.");
        localStorage.removeItem(tokenKey);
      }
    }

    return response;
  };

  // Function to refresh access token
  async function refreshAccessToken() {
    try {
      const response = await originalFetch(refreshUrl, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
      });

      if (response.ok) {
        const data = await response.json();
        if (data.token) {
          localStorage.setItem(tokenKey, data.token);
          return data.token;
        }
      }
    } catch (error) {
      console.error("[custom-auth.js]: Error refreshing token", error);
    }
    return null;
  }

  // Function to retry the original request with a new token
  async function retryRequest(url, options, token) {
    const newOptions = { ...options };
    newOptions.headers = {
      ...newOptions.headers,
      Authorization: "Bearer " + token,
    };
    return await originalFetch(url, newOptions);
  }

  // Function to set token in Swagger UI
  function setSwaggerAuth(token) {
    const bearerToken = "Bearer " + token;
    localStorage.setItem(tokenKey, bearerToken);
    const ui = window.ui || window.swaggerUIBundle || window.swaggerUi;
    if (ui) {
      ui.preauthorizeApiKey("Bearer", bearerToken);
    }
  }

  // Set token from localStorage on page load
  document.addEventListener("DOMContentLoaded", () => {
    const savedToken = localStorage.getItem(tokenKey);
    if (savedToken) {
      setSwaggerAuth(savedToken);
    }
  });
})();
