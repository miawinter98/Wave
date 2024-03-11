﻿using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Wave.Data;

public class UserClaimsFactory(
	UserManager<ApplicationUser> userManager,
	RoleManager<IdentityRole> roleManager,
	IOptions<IdentityOptions> options)
	: UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>(userManager, roleManager, options) {
	protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user) {
		var principal = await base.GenerateClaimsAsync(user);
		// principal.AddClaim(new Claim("Id", user.Id));
		principal.AddClaim(new Claim("FullName", user.Name));
		return principal;
	}
}