# Authentication Configuration

## Google OAuth Setup

To enable Google authentication, you need to set up a Google Cloud project and obtain OAuth 2.0 credentials:

### 1. Create a Google Cloud Project

1. Go to the [Google Cloud Console](https://console.cloud.google.com/)
2. Create a new project or select an existing one
3. Enable the Google+ API (or People API)

### 2. Create OAuth 2.0 Credentials

1. Navigate to **APIs & Services** > **Credentials**
2. Click **Create Credentials** > **OAuth client ID**
3. Select **Web application**
4. Add authorized redirect URIs:
   - For local development: `http://localhost:3000/api/auth/google-callback`
   - For local development with backend: `http://localhost:8080/api/auth/google-callback`
   - For production: `https://yourdomain.com/api/auth/google-callback`
5. Save and copy the **Client ID** and **Client Secret**

### 3. Configure the Application

**For local development:**

Add your credentials to `backend/CoParenting.API/appsettings.Development.json`:

```json
{
  "Authentication": {
    "Google": {
      "ClientId": "your-client-id-here.apps.googleusercontent.com",
      "ClientSecret": "your-client-secret-here"
    }
  }
}
```

**For Docker/Production:**

Set environment variables in `docker-compose.yml`:

```yaml
services:
  app:
    environment:
      - Authentication__Google__ClientId=your-client-id
      - Authentication__Google__ClientSecret=your-client-secret
```

Or use environment variables:

```bash
export Authentication__Google__ClientId="your-client-id"
export Authentication__Google__ClientSecret="your-client-secret"
```

**Note:** Never commit real credentials to source control. The empty strings in `appsettings.json` are placeholders.

## Demo Mode

The application includes a demo mode that doesn't require Google authentication:

- Click "Try Demo Mode" on the login screen
- Creates a temporary demo user and child
- Demo data is isolated and temporary
- Perfect for testing and demonstrations

## Security Considerations

- Always use HTTPS in production
- Keep your Client Secret secure
- Regularly rotate credentials
- Review Google Cloud Console security settings
- Monitor authentication logs for suspicious activity

## Troubleshooting

**"Redirect URI mismatch" error:**
- Verify the redirect URI in Google Cloud Console matches exactly
- Check that you're using the correct port (3000 for production, 8080 for dev)

**Authentication not working:**
- Verify environment variables are set correctly
- Check that Google+ API or People API is enabled
- Ensure cookies are enabled in the browser
- Check CORS configuration if running separately

**Demo mode not working:**
- Check database connectivity
- Verify that the Users and Children tables exist
- Check application logs for errors
